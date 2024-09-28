using BusinessObjects.Dtos.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Dtos.Commons;
using BusinessObjects.Dtos.Refunds;
using Repositories.Accounts;
using BusinessObjects.Entities;
using BusinessObjects.Utils;
using Dao;
using DotNext;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.Attr;
using Repositories.Auctions;
using Repositories.ConsignSaleLineItems;
using Repositories.ConsignSales;
using Repositories.Orders;
using Repositories.Refunds;

namespace Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IConsignSaleRepository _consignSaleRepository;
        private readonly IRefundRepository _refundRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IConsignSaleLineItemRepository _consignSaleLineItemRepository;

        public EmailService(IConfiguration configuration, IAccountRepository accountRepository,
            IOrderRepository orderRepository,
            IConsignSaleRepository consignSaleRepository, IRefundRepository refundRepository, IAuctionRepository auctionRepository,
            IConsignSaleLineItemRepository consignSaleLineItemRepository)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _consignSaleRepository = consignSaleRepository;
            _refundRepository = refundRepository;
            _auctionRepository = auctionRepository;
            _consignSaleLineItemRepository = consignSaleLineItemRepository;
        }

        public string GetEmailTemplate(string templateName)
        {
            // string pathLocal = Path.Combine("C:\\FPT_University_FULL\\CAPSTONE_API\\Services\\MailTemplate\\", $"{templateName}.html");*/
            string path = Path.Combine(_configuration.GetSection("EmailTemplateDirectory").Value!,
                $"{templateName}.html");
            var template = File.ReadAllText(path, Encoding.UTF8);
            template = template.Replace("[path]", _configuration.GetSection("RedirectUrl").Value);
            return template;
        }

        public async Task SendEmail(SendEmailRequest request)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_configuration.GetSection("MailSettings:Mail").Value));
            email.To.Add(MailboxAddress.Parse(request.To));
            email.Subject = request.Subject;
            email.Body = new TextPart(TextFormat.Html) { Text = request.Body };


            // dùng SmtpClient của MailKit
            using var smtp = new SmtpClient();
            
            await smtp.ConnectAsync(_configuration.GetSection("MailSettings:Host").Value, 587,
                SecureSocketOptions.Auto);
            await smtp.AuthenticateAsync(_configuration.GetSection("MailSettings:Mail").Value,
                _configuration.GetSection("MailSettings:Password").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<string>> SendMailRegister(string email, string token)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<string>();
            var user = await _accountRepository.FindUserByEmail(email);
            string appDomain = _configuration.GetSection("MailSettings:AppDomain").Value;
            string confirmationLink = _configuration.GetSection("MailSettings:EmailConfirmation").Value;
            string formattedLink = string.Format(appDomain + confirmationLink, user.AccountId, token);

            var template = GetEmailTemplate("VerifyAccountMail");
            template = template.Replace($"[link]", formattedLink);

            SendEmailRequest content = new SendEmailRequest
            {
                To = email,
                Subject = "[GIVEAWAY] Verify Account",
                Body = template,
            };
            await SendEmail(content);
            // response.Messages = [""];
            response.ResultStatus = ResultStatus.Success;
            return response;
        }

        public async Task<bool> SendEmailOrder(Order order)
        {
            
            SendEmailRequest content = new SendEmailRequest();
            if (order.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(order.MemberId.Value);
                content.To = member!.Email;
                List<OrderLineItem> listOrderLineItems = order.OrderLineItems.ToList();
                string orderTemplate = @"
<table align='center' border='0' cellpadding='0' cellspacing='0' class='row row-5' role='presentation'
    style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;' width='100%'>
    <tbody>
    <tr>
        <td>
            <table align='center' border='0' cellpadding='0' cellspacing='0' class='row-content stack'
                   role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #060e21; background-image: url(https://firebasestorage.googleapis.com/v0/b/give-away-a58b2.appspot.com/o/images%2Flogo%2F16a4fb29-7166-41c8-8dcd-c438768c806f.jpg?alt=media&token=fceb70e6-8bf8-484a-bc75-c18cfd8edd9a); color: #000000; width: 650px; margin: 0 auto;' width='650'>
                <tbody>
                <tr>
                    <td class='column column-1' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='image_block block-1' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='width: 100%; padding-right: 0px; padding-left: 0px;'>
                                    <div align='center' class='alignment' style='line-height: 10px'>
                                        <div style='max-width: 242.25px'>
                                            <img alt='{PRODUCT_NAME}' height='100px' src='{PRODUCT_IMAGE_URL}' style='display: block; height: auto; border: 0; width: 70%;' title='{PRODUCT_NAME}' width='242.25'/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td class='column column-2' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='heading_block block-2' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px; text-align: center; width: 100%;'>
                                    <h2 style='margin: 0; color: #b23ab6; direction: ltr; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 24px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 28.799999999999997px;'>
                                        <strong>{PRODUCT_NAME}</strong>
                                    </h2>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-3' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>{SELLING_PRICE} VND</span><br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-4' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px; padding-top: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald ,Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Quantity:</span> {QUANTITY}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Color: </span> {COLOR}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Condition: </span> {Condition}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                </tbody>
            </table>
        </td>
    </tr>
    </tbody>
</table>";


                var template = GetEmailTemplate("OrderMail");
                StringBuilder htmlBuilder = new StringBuilder();

                foreach (var item in listOrderLineItems)
                {
                    string filledTemplate = orderTemplate
                        .Replace("{PRODUCT_NAME}", item.IndividualFashionItem.MasterItem.Name)
                        .Replace("{QUANTITY}", item.Quantity.ToString())
                        .Replace("{COLOR}", item.IndividualFashionItem.Color)
                        .Replace("{Condition}", item.IndividualFashionItem.Condition)
                        .Replace("{PRODUCT_IMAGE_URL}", item.IndividualFashionItem.Images.Select(c => c.Url).FirstOrDefault())
                        .Replace("{SELLING_PRICE}", item.IndividualFashionItem.SellingPrice!.Value.ToString("N0"));

                    htmlBuilder.Append(filledTemplate);
                }

                string finalHtml = htmlBuilder.ToString();
                template = template.Replace($"[Order Code]", order.OrderCode);
                template = template.Replace($"[Quantity]", order.OrderLineItems.Count().ToString());
                template = template.Replace($"[Payment Method]", order.PaymentMethod.ToString());
                template = template.Replace($"[Order Template]", finalHtml);
                template = template.Replace($"[Total Price]", order.TotalPrice.ToString("N0"));
                template = template.Replace($"[Recipient Name]", order.RecipientName);
                template = template.Replace($"[Phone Number]", order.Phone);
                template = template.Replace($"[Email]", order.Email);
                template = template.Replace($"[Address]", order.Address);
                template = template.Replace($"[Shipping Fee]", order.ShippingFee.ToString("N0")) ?? "N/A";
                template = template.Replace($"[Discount]", order.Discount.ToString("N0"));
                template = template.Replace($"[Payment Date]",
                    order.OrderLineItems.Select(c => c.PaymentDate).FirstOrDefault()!.Value.AddHours(7).ToString("G")) ?? "N/A";
                content.Subject = $"[GIVEAWAY] ORDER INVOICE FROM GIVEAWAY";
                content.Body = template;
                await SendEmail(content);
                return true;
            }
            return false;
        }

        public async Task<bool> SendEmailRefund(Guid refundId)
        {
            try
            {
                SendEmailRequest content = new SendEmailRequest();
                Expression<Func<Refund, bool>> predicate = refund => refund.RefundId == refundId;
                var refund = await _refundRepository.GetSingleRefund(predicate);
                if (refund is null)
                {
                    throw new RefundNotFoundException();
                }

                string imageRefundTemplate =
                    @"<td style='padding-bottom:10px;padding-right:16px;padding-top:10px;'>
								<div style='max-width: 180px'>
										<img alt='{refund_url}' height='100px' src='{REFUND_URL}' style='display: flex; margin-right: 10px; height: auto; border: 0; width: 80%;' title='{PRODUCT_NAME}' width='242.25'/>
								</div>
					</td>";
                StringBuilder htmlBuilder = new StringBuilder();
                foreach (var refundImage in refund.Images)
                {
                    string filledTemplate = imageRefundTemplate
                        .Replace("{REFUND_URL}", refundImage.Url);
                    htmlBuilder.Append(filledTemplate);
                }
                string finalHtml = htmlBuilder.ToString();
                var order = refund.OrderLineItem.Order;
                var item = refund.OrderLineItem.IndividualFashionItem;
                var refundAmount = refund.OrderLineItem.UnitPrice * refund.RefundPercentage / 100;
                var template = GetEmailTemplate("RefundMail");
                template = template.Replace("{PRODUCT_NAME}", item.MasterItem.Name);
                template = template.Replace("{COLOR}", item.Color);
                template = template.Replace("{SIZE}", item.Size.ToString());
                template = template.Replace("{Condition}", item.Condition);
                template = template.Replace("{GENDER}", item.MasterItem.Gender.ToString());
                template = template.Replace("{NOTE}", item.Note);
                template = template.Replace("{PRODUCT_IMAGE_URL}",
                    item.Images.Select(c => c.Url).FirstOrDefault());
                template = template.Replace("{SELLING_PRICE}", item.SellingPrice!.Value.ToString("N0"));    
                template = template.Replace("[ListRefundImages]", finalHtml);    
                template = template.Replace("[Order Code]", order!.OrderCode);
                template = template.Replace("[Status]", refund.RefundStatus.ToString());
                template = template.Replace("[Product Name]", item.MasterItem.Name);
                template = template.Replace("[Created Date]", refund.CreatedDate.AddHours(7).ToString("G"));
                template = template.Replace("[Refund Percent]", refund.RefundPercentage!.Value.ToString());
                template = template.Replace("[Refund Amount]", refundAmount!.Value.ToString("N0"));
            
                template = template.Replace("[Description]", refund.Description);
                template = template.Replace("[Response]", refund.ResponseFromShop);
                if (order.MemberId != null)
                {
                    var member = await GenericDao<Account>.Instance.GetQueryable().Where(c => c.AccountId == order.MemberId)
                        .FirstOrDefaultAsync();
                    if (member is null)
                    {
                        return false;
                    }
                    template = template.Replace("[Customer Name]", member.Fullname);
                    template = template.Replace("[Phone Number]", member.Phone);
                    template = template.Replace("[Email]", member.Email);
                    content.To = member.Email;
                    content.Subject = $"[GIVEAWAY] REFUND RESPONSE FROM GIVEAWAY";
                    content.Body = template;

                    await SendEmail(content);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SendEmailConsignSale(Guid consignSaleId)
        {
            try
            {
                Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
                var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
                // List<ConsignSaleLineItem> listConsignSaleLine = consignSale!.ConsignSaleLineItems.ToList();
                string consignTemplate = @"
            <table align='center' border='0' cellpadding='0' cellspacing='0' class='row row-5' role='presentation'
						   style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;' width='100%'>
						<tbody>
						<tr>
							<td>
								<table align='center' border='0' cellpadding='0' cellspacing='0' class='row-content stack'
									   role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #060e21; background-image: url(https://firebasestorage.googleapis.com/v0/b/give-away-a58b2.appspot.com/o/images%2Flogo%2F16a4fb29-7166-41c8-8dcd-c438768c806f.jpg?alt=media&token=fceb70e6-8bf8-484a-bc75-c18cfd8edd9a); color: #000000; width: 650px; margin: 0 auto;' width='650'>
									<tbody>
									<tr>
										<td class='column column-1' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
											<table border='0' cellpadding='0' cellspacing='0' class='image_block block-1' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
												<tr>
													<td class='pad' style='width: 100%; padding-right: 0px; padding-left: 0px;'>
														<div align='center' class='alignment' style='line-height: 10px'>
															<div style='max-width: 242.25px'>
																<img alt='{PRODUCT_NAME}' height='100px' src='{PRODUCT_IMAGE_URL}' style='display: block; height: auto; border: 0; width: 70%;' title='{PRODUCT_NAME}' width='242.25'/>
															</div>
														</div>
													</td>
												</tr>
											</table>
										</td>
										<td class='column column-2' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
											<table border='0' cellpadding='0' cellspacing='0' class='heading_block block-2' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px; text-align: center; width: 100%;'>
														<h2 style='margin: 0; color: #b23ab6; direction: ltr; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 24px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 28.799999999999997px;'>
															<strong>{PRODUCT_NAME}</strong>
														</h2>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-3' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Expected Price: </span>{EXPECTED_PRICE} VND<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-4' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px; padding-top: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Gender: </span>{GENDER}<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Color: </span> {COLOR}<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Condition: </span> {Condition}<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-left: 10px; padding-right: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Size: </span> {SIZE}<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
											<table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
												<tr>
													<td class='pad' style='padding-bottom: 10px; padding-left: 10px; padding-right: 10px;'>
														<div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
														<p style='margin: 0; word-break: break-word'>
															<span style='word-break: break-word; color: #b23ab6;'>Note: </span> {NOTE}<br/>
														</p>
														</div>
													</td>
												</tr>
											</table>
										</td>
									</tr>
									</tbody>
								</table>
							</td>
						</tr>
						</tbody>
					</table>";
                SendEmailRequest content = new SendEmailRequest();
                StringBuilder htmlBuilder = new StringBuilder();
                if (consignSale is null)
                {
                    Console.WriteLine("Consign is not found");
                    return false;
                }
                foreach (var item in consignSale.ConsignSaleLineItems)
                {
                    string filledTemplate = consignTemplate
                        .Replace("{PRODUCT_NAME}", item.ProductName)
                        .Replace("{GENDER}", item.Gender.ToString())
                        .Replace("{SIZE}", item.Size.ToString())
                        .Replace("{COLOR}", item.Color)
                        .Replace("{NOTE}", item.Note)
                        .Replace("{Condition}", item.Condition)
                        .Replace("{PRODUCT_IMAGE_URL}", item.Images.Select(c => c.Url).FirstOrDefault())
                        .Replace("{EXPECTED_PRICE}", item.ExpectedPrice.ToString("N0"));

                    htmlBuilder.Append(filledTemplate);
                }

                string finalHtml = htmlBuilder.ToString();
                if (consignSale.MemberId != null)
                {
                    var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                    content.To = member!.Email;

                    var template = GetEmailTemplate("ConsignSaleMail");
                    template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                    template = template.Replace("[Type]", consignSale.Type.ToString());
                    template = template.Replace("[Created Date]", consignSale.CreatedDate.AddHours(7).ToString("G"));
                    template = template.Replace("[Customer Name]", consignSale.ConsignorName);
                    template = template.Replace("[Phone Number]", consignSale.Phone);
                    template = template.Replace("[ConsignTemplate]", finalHtml);
                    template = template.Replace("[Email]", consignSale.Email);
                    template = template.Replace("[ShopAddress]", consignSale.Shop.Address);
                    if (consignSale.Status.Equals(ConsignSaleStatus.AwaitDelivery))
                    {
                        template = template.Replace("[Status]", "Approved");
                        template = template.Replace("[Response]",
                            "Please deliver your products to our shop as soon as possible");
                        template = template.Replace("[ConsignSale Duration]", "60 Days");
                    }
                    else
                    {
                        template = template.Replace("[Status]", "Rejected");
                        template = template.Replace("[Response]",
                            "Your products is kindly not suitable to our shop. We are so apologize");
                        template = template.Replace("[ConsignSale Duration]", "0 Day");
                    }

                    content.Subject = $"[GIVEAWAY] CONSIGN ANNOUNCEMENT FROM GIVEAWAY";
                    content.Body = template;

                    await SendEmail(content);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return false;
        }

        public async Task<BusinessObjects.Dtos.Commons.Result<string>> SendMailForgetPassword(string email)
        {
            var response = new BusinessObjects.Dtos.Commons.Result<string>();
            var account = await _accountRepository.FindUserByEmail(email);
            var user = await _accountRepository.ResetPasswordToken(account);
            response.Data = user!.PasswordResetToken!;
            var template = GetEmailTemplate("ForgotPasswordMail");
            template = template.Replace("[Token]", response.Data);
            SendEmailRequest content = new SendEmailRequest
            {
                To = $"{account!.Email}",
                Subject = "[GIVEAWAY] RESET PASSWORD",
                Body = template
            };
            await SendEmail(content);
            response.Messages = ["Please check your mail to get the token to reset password"];
            response.ResultStatus = ResultStatus.Success;

            return response;
        }

        public async Task<bool> SendEmailConsignSaleReceived(ConsignSale consignSale)
        {
            
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member!.Email;

                var template = GetEmailTemplate("ConsignSaleReceivedMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[CreatedDate]", consignSale.CreatedDate.AddHours(7).ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.ConsignorName);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[ReceivedAt]", DateTime.UtcNow.AddHours(7).ToString("G"));
                template = template.Replace("[Response]",
                    "Thank you for trusting and using the consignment service at Give Away store.");
                template = template.Replace("[ShopAddress]", consignSale.Shop.Address);

                content.Subject = $"[GIVEAWAY] RECEIVED CONSIGN FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }

            return false;
        }

        public async Task<bool> SendEmailConsignSaleEndedMail(Guid consignSaleId)
        {
            try
            {
                Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
                var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
                SendEmailRequest content = new SendEmailRequest();
                if (consignSale != null)
                {
                
                    content.To = consignSale.Member!.Email;

                    var template = GetEmailTemplate("ConsignSaleEndedMail");
                    template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                    template = template.Replace("[Type]", consignSale.Type.ToString());
                    template = template.Replace("[Total Price]", consignSale.TotalPrice.ToString("N0"));
                    template = template.Replace("[Sold Price]", consignSale.SoldPrice.ToString("N0"));
                    template = template.Replace("[Amount Receive]", consignSale.ConsignorReceivedAmount.ToString("N0"));
                    template = template.Replace("[Phone Number]", consignSale.Phone);
                    template = template.Replace("[Email]", consignSale.Email);
                    template = template.Replace("[Address]", consignSale.Address);
                    template = template.Replace("[Response]",
                        "Thank you for trusting and using the consignment service at Give Away store.");
                    if (consignSale.SoldPrice < 1000000)
                    {
                        template = template.Replace("[Consignment Fee]", "26%");
                    }
                    else if (consignSale.SoldPrice >= 1000000 && consignSale.SoldPrice <= 10000000)
                    {
                        template = template.Replace("[Consignment Fee]", "23%");
                    }
                    else
                    {
                        template = template.Replace("[Consignment Fee]", "20%");
                    }

                    content.Subject = $"[GIVEAWAY] CONSIGNMENT ENDED FROM GIVEAWAY";
                    content.Body = template;

                    await SendEmail(content);
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                return false;
            }

           
        }

        public async Task<bool> SendEmailAuctionIsComing(Guid auctionId, Guid memberId)
        {
            try
            {
                var auction = await _auctionRepository.GetAuction(auctionId, true);
                if (auction is null)
                {
                    return false;
                }
                var member = auction.AuctionDeposits.Where(c => c.MemberId == memberId).Select(c => c.Member)
                    .FirstOrDefault();
                if (member is null)
                {
                    return false;
                }
                SendEmailRequest content = new SendEmailRequest
                {
                    Subject = $"[GIVEAWAY] AUCTION IS COMING"
                };
                var template = GetEmailTemplate("AuctionComingMail");
                template = template.Replace("{PRODUCT_NAME}", auction.IndividualAuctionFashionItem.MasterItem.Name);
                template = template.Replace("{INITIAL_PRICE}", auction.IndividualAuctionFashionItem.InitialPrice!.Value.ToString("N0"));
                template = template.Replace("{PRODUCT_IMAGE_URL}", auction.IndividualAuctionFashionItem.Images.Select(c => c.Url).FirstOrDefault());
                template = template.Replace("{GENDER}", auction.IndividualAuctionFashionItem.MasterItem.Gender.ToString());
                template = template.Replace("{COLOR}", auction.IndividualAuctionFashionItem.Color);
                template = template.Replace("{Condition}", auction.IndividualAuctionFashionItem.Condition);
                template = template.Replace("{SIZE}", auction.IndividualAuctionFashionItem.Size.ToString());
                template = template.Replace("{NOTE}", auction.IndividualAuctionFashionItem.Note);
                template = template.Replace("[Title]", auction.Title);
                template = template.Replace("[Auction Code]", auction.AuctionCode);
                template = template.Replace("[StartTime]", auction.StartDate.AddHours(7).ToString("G"));
                template = template.Replace("[EndTime]", auction.EndDate.AddHours(7).ToString("G"));
                    
                content.To = member.Email;
                template = template.Replace("[Customer Name]", member.Fullname);
                template = template.Replace("[Email]", member.Email);
                template = template.Replace("[Phone Number]", member.Phone);
                content.Body = template;
                await SendEmail(content);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SendEmailAuctionWon(Guid auctionId, Bid bid)
        {
            try
            {
                var auction = await _auctionRepository.GetAuction(auctionId, true);
                if (auction is null)
                {
                    return false;
                }
                var member = auction.AuctionDeposits.Where(c => c.MemberId == bid.MemberId).Select(c => c.Member)
                    .FirstOrDefault();
                if (member is null)
                {
                    return false;
                }
                SendEmailRequest content = new SendEmailRequest
                {
                    Subject = $"[GIVEAWAY] CONGRATULATION AUCTION WINNER"
                };
                var template = GetEmailTemplate("AuctionWonMail");
                template = template.Replace("{PRODUCT_NAME}", auction.IndividualAuctionFashionItem.MasterItem.Name);
                template = template.Replace("{INITIAL_PRICE}", auction.IndividualAuctionFashionItem.InitialPrice!.Value.ToString("N0"));
                template = template.Replace("{PRODUCT_IMAGE_URL}", auction.IndividualAuctionFashionItem.Images.Select(c => c.Url).FirstOrDefault());
                template = template.Replace("{GENDER}", auction.IndividualAuctionFashionItem.MasterItem.Gender.ToString());
                template = template.Replace("{COLOR}", auction.IndividualAuctionFashionItem.Color);
                template = template.Replace("{Condition}", auction.IndividualAuctionFashionItem.Condition);
                template = template.Replace("{SIZE}", auction.IndividualAuctionFashionItem.Size.ToString());
                template = template.Replace("{NOTE}", auction.IndividualAuctionFashionItem.Note);
                template = template.Replace("[Title]", auction.Title);
                template = template.Replace("[Auction Code]", auction.AuctionCode);
                template = template.Replace("[StartTime]", auction.StartDate.AddHours(7).ToString("G"));
                template = template.Replace("[EndTime]", auction.EndDate.AddHours(7).ToString("G"));
                template = template.Replace("[WonPrice]", bid.Amount.ToString("N0"));
                    
                content.To = member.Email;
                template = template.Replace("[Customer Name]", member.Fullname);
                template = template.Replace("[Email]", member.Email);
                template = template.Replace("[Phone Number]", member.Phone);
                content.Body = template;
                await SendEmail(content);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SendMailSoldItemConsign(Guid consignLineId, decimal amountConsignorReceive)
        {
            try
            {
                Expression<Func<ConsignSaleLineItem, bool>> predicate = x => x.ConsignSaleLineItemId == consignLineId;
                var consignSaleLineItem = await _consignSaleLineItemRepository.GetSingleConsignSaleLineItem(predicate);
                if (consignSaleLineItem is null)
                {
                    return false;
                }

                var consignSale = consignSaleLineItem.ConsignSale;
                var member = await _accountRepository.GetAccountById(consignSaleLineItem.ConsignSale.MemberId!.Value);
                if (member is null)
                {
                    return false;
                }
                SendEmailRequest content = new SendEmailRequest
                {
                    Subject = $"[GIVEAWAY] CONSIGN PRODUCT SOLD ANNOUNCEMENT",
                    To = member.Email
                };
                var template = GetEmailTemplate("SoldItemConsignMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[SoldPrice]", consignSaleLineItem.ConfirmedPrice!.Value.ToString("N0"));
                template = template.Replace("[ConsignorReceive]", amountConsignorReceive.ToString("N0"));
                template = template.Replace("[ConsignmentFee]", "20%");
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                template = template.Replace("[Customer Name]", consignSale.ConsignorName);
                
                template = template.Replace("{PRODUCT_NAME}", consignSaleLineItem.ProductName);
                template = template.Replace("{COLOR}", consignSaleLineItem.Color);
                template = template.Replace("{EXPECTED_PRICE}", consignSaleLineItem.ConfirmedPrice!.Value.ToString("N0"));
                template = template.Replace("{SIZE}", consignSaleLineItem.Size.ToString());
                template = template.Replace("{Condition}", consignSaleLineItem.Condition);
                template = template.Replace("{GENDER}", consignSaleLineItem.Gender.ToString());
                template = template.Replace("{NOTE}", consignSaleLineItem.Note);
                template = template.Replace("{PRODUCT_IMAGE_URL}",
                    consignSaleLineItem.Images.Select(c => c.Url).FirstOrDefault());
                content.Body = template;
                await SendEmail(content);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SendEmailCancelOrderAndReservedItems(Order order)
        {
            try
            {
                var content = new SendEmailRequest();
                if (order.MemberId != null)
                {
                    var member = await _accountRepository.GetAccountById(order.MemberId.Value);
                    content.To = member!.Email;
                    List<IndividualFashionItem> listIndividualReserved = order.OrderLineItems.Where(c => c.IndividualFashionItem.Status == FashionItemStatus.Reserved)
                        .Select(c => c.IndividualFashionItem).ToList();
                    string ListReservedItems = @"
<table align='center' border='0' cellpadding='0' cellspacing='0' class='row row-5' role='presentation'
    style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;' width='100%'>
    <tbody>
    <tr>
        <td>
            <table align='center' border='0' cellpadding='0' cellspacing='0' class='row-content stack'
                   role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #060e21; background-image: url(https://firebasestorage.googleapis.com/v0/b/give-away-a58b2.appspot.com/o/images%2Flogo%2F16a4fb29-7166-41c8-8dcd-c438768c806f.jpg?alt=media&token=fceb70e6-8bf8-484a-bc75-c18cfd8edd9a); color: #000000; width: 650px; margin: 0 auto;' width='650'>
                <tbody>
                <tr>
                    <td class='column column-1' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='image_block block-1' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='width: 100%; padding-right: 0px; padding-left: 0px;'>
                                    <div align='center' class='alignment' style='line-height: 10px'>
                                        <div style='max-width: 242.25px'>
                                            <img alt='{PRODUCT_NAME}' height='100px' src='{PRODUCT_IMAGE_URL}' style='display: block; height: auto; border: 0; width: 70%;' title='{PRODUCT_NAME}' width='242.25'/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td class='column column-2' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='heading_block block-2' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px; text-align: center; width: 100%;'>
                                    <h2 style='margin: 0; color: #b23ab6; direction: ltr; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 24px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 28.799999999999997px;'>
                                        <strong>{PRODUCT_NAME}</strong>
                                    </h2>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-3' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>{SELLING_PRICE} VND</span><br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-4' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px; padding-top: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald ,Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Quantity:</span> {QUANTITY}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Color: </span> {COLOR}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Condition: </span> {Condition}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                </tbody>
            </table>
        </td>
    </tr>
    </tbody>
</table>";

                    var template = GetEmailTemplate("CancelOrderAndReservedItemMail");
                    StringBuilder htmlBuilderReserved = new StringBuilder();

                    foreach (var item in listIndividualReserved)
                    {
                        string filledTemplate = ListReservedItems
                            .Replace("{PRODUCT_NAME}", item.MasterItem.Name)
                            .Replace("{QUANTITY}", "1")
                            .Replace("{COLOR}", item.Color)
                            .Replace("{Condition}", item.Condition)
                            .Replace("{PRODUCT_IMAGE_URL}", item.Images.Select(c => c.Url).FirstOrDefault())
                            .Replace("{SELLING_PRICE}", item.SellingPrice!.Value.ToString("N0"));

                        htmlBuilderReserved.Append(filledTemplate);
                    }
                    string finalReservedHtml = htmlBuilderReserved.ToString();
                
                    List<IndividualFashionItem> listIndividualUnavailable = order.OrderLineItems.Where(c => c.IndividualFashionItem.Status == FashionItemStatus.Unavailable)
                        .Select(c => c.IndividualFashionItem).ToList();
                    string ListUnavailableItems = @"
<table align='center' border='0' cellpadding='0' cellspacing='0' class='row row-5' role='presentation'
    style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;' width='100%'>
    <tbody>
    <tr>
        <td>
            <table align='center' border='0' cellpadding='0' cellspacing='0' class='row-content stack'
                   role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #060e21; background-image: url(https://firebasestorage.googleapis.com/v0/b/give-away-a58b2.appspot.com/o/images%2Flogo%2F16a4fb29-7166-41c8-8dcd-c438768c806f.jpg?alt=media&token=fceb70e6-8bf8-484a-bc75-c18cfd8edd9a); color: #000000; width: 650px; margin: 0 auto;' width='650'>
                <tbody>
                <tr>
                    <td class='column column-1' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='image_block block-1' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='width: 100%; padding-right: 0px; padding-left: 0px;'>
                                    <div align='center' class='alignment' style='line-height: 10px'>
                                        <div style='max-width: 242.25px'>
                                            <img alt='{PRODUCT_NAME_UN}' height='100px' src='{PRODUCT_IMAGE_URL_UN}' style='display: block; height: auto; border: 0; width: 70%;' title='{PRODUCT_NAME}' width='242.25'/>
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                    <td class='column column-2' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                        <table border='0' cellpadding='0' cellspacing='0' class='heading_block block-2' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px; text-align: center; width: 100%;'>
                                    <h2 style='margin: 0; color: #b23ab6; direction: ltr; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 24px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 28.799999999999997px;'>
                                        <strong>{PRODUCT_NAME_UN}</strong>
                                    </h2>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-3' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>{SELLING_PRICE_UN} VND</span><br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-4' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px; padding-top: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald ,Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Quantity:</span> {QUANTITY_UN}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Color: </span> {COLOR_UN}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                        <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                            <tr>
                                <td class='pad' style='padding-bottom: 20px; padding-left: 10px; padding-right: 10px;'>
                                    <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                        <p style='margin: 0; word-break: break-word'>
                                            <span style='word-break: break-word; color: #b23ab6;'>Condition: </span> {Condition_UN}<br/>
                                        </p>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </td>
                </tr>
                </tbody>
            </table>
        </td>
    </tr>
    </tbody>
</table>";

                    StringBuilder htmlBuilderUnavailable = new StringBuilder();

                    foreach (var item in listIndividualUnavailable)
                    {
                        string filledTemplate = ListUnavailableItems
                            .Replace("{PRODUCT_NAME_UN}", item.MasterItem.Name)
                            .Replace("{QUANTITY_UN}", "1")
                            .Replace("{COLOR_UN}", item.Color)
                            .Replace("{Condition_UN}", item.Condition)
                            .Replace("{PRODUCT_IMAGE_URL_UN}", item.Images.Select(c => c.Url).FirstOrDefault())
                            .Replace("{SELLING_PRICE_UN}", item.SellingPrice!.Value.ToString("N0"));

                        htmlBuilderUnavailable.Append(filledTemplate);
                    }
                    string finalUnavailableHtml = htmlBuilderUnavailable.ToString();
                
                    template = template.Replace($"[Order Code]", order.OrderCode);
                    template = template.Replace($"[Quantity]", order.OrderLineItems.Count().ToString());
                    template = template.Replace($"[Payment Method]", order.PaymentMethod.ToString());
                    template = template.Replace($"[ListReservedItems]", finalReservedHtml);
                    template = template.Replace($"[ListUnavailableItems]", finalUnavailableHtml);
                    template = template.Replace($"[Total Price]", order.TotalPrice.ToString("N0"));
                    template = template.Replace($"[Recipient Name]", order.RecipientName);
                    template = template.Replace($"[Phone Number]", order.Phone);
                    template = template.Replace($"[Email]", order.Email);
                    template = template.Replace($"[Address]", order.Address);
                    template = template.Replace($"[Shipping Fee]", order.ShippingFee.ToString("N0")) ?? "N/A";
                    template = template.Replace($"[Discount]", order.Discount.ToString("N0"));
                    template = template.Replace($"[Payment Date]",
                        order.OrderLineItems.Select(c => c.PaymentDate).FirstOrDefault()!.Value.AddHours(7).ToString("G")) ?? "N/A";
                    content.Subject = $"[GIVEAWAY] ORDER INVOICE FROM GIVEAWAY";
                    content.Body = template;
                    await SendEmail(content);
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> SendEmailConsignNegotiatePrice(ConsignSale consignSale)
        {
            List<ConsignSaleLineItem> listConsignSaleLine = consignSale.ConsignSaleLineItems.Where(c => c.Status == ConsignSaleLineItemStatus.Negotiating).ToList();
            string consignTemplate = @"
            <table align='center' border='0' cellpadding='0' cellspacing='0' class='row row-5' role='presentation'
                   style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;' width='100%'>
                <tbody>
                <tr>
                    <td>
                        <table align='center' border='0' cellpadding='0' cellspacing='0' class='row-content stack'
                               role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #060e21; background-image: url(https://firebasestorage.googleapis.com/v0/b/give-away-a58b2.appspot.com/o/images%2Flogo%2F16a4fb29-7166-41c8-8dcd-c438768c806f.jpg?alt=media&token=fceb70e6-8bf8-484a-bc75-c18cfd8edd9a); color: #000000; width: 650px; margin: 0 auto;' width='650'>
                            <tbody>
                            <tr>
                                <td class='column column-1' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                                    <table border='0' cellpadding='0' cellspacing='0' class='image_block block-1' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                                        <tr>
                                            <td class='pad' style='width: 100%; padding-right: 0px; padding-left: 0px;'>
                                                <div align='center' class='alignment' style='line-height: 10px'>
                                                    <div style='max-width: 242.25px'>
                                                        <img alt='{PRODUCT_NAME}' height='100px' src='{PRODUCT_IMAGE_URL}' style='display: block; height: auto; border: 0; width: 70%;' title='{PRODUCT_NAME}' width='242.25'/>
                                                    </div>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                                <td class='column column-2' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-left: 20px; padding-right: 20px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;' width='50%'>
                                    <table border='0' cellpadding='0' cellspacing='0' class='heading_block block-2' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px; text-align: center; width: 100%;'>
                                                <h2 style='margin: 0; color: #b23ab6; direction: ltr; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 24px; font-weight: normal; letter-spacing: normal; line-height: 120%; text-align: left; margin-top: 0; margin-bottom: 0; mso-line-height-alt: 28.799999999999997px;'>
                                                    <strong>{PRODUCT_NAME}</strong>
                                                </h2>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-3' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Expected Price: </span>{EXPECTED_PRICE} VND<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Deal Price: </span>{DEAL_PRICE} VND<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-4' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px; padding-top: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Helvetica Neue, Helvetica, Arial, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Gender: </span>{GENDER}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Color: </span> {COLOR}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Condition: </span> {Condition}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Size: </span> {SIZE}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-bottom: 10px; padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Note: </span> {NOTE}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                    <table border='0' cellpadding='0' cellspacing='0' class='paragraph_block block-6' role='presentation' style='mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;' width='100%'>
                                        <tr>
                                            <td class='pad' style='padding-bottom: 10px; padding-left: 10px; padding-right: 10px;'>
                                                <div style='color: #393d47; font-family: Oswald, Arial, Helvetica Neue, Helvetica, sans-serif; font-size: 17px; letter-spacing: 0px; line-height: 150%; text-align: left; mso-line-height-alt: 25.5px;'>
                                                    <p style='margin: 0; word-break: break-word'>
                                                        <span style='word-break: break-word; color: #b23ab6;'>Response: </span> {RESPONSEFROMSHOP}<br/>
                                                    </p>
                                                </div>
                                            </td>
                                        </tr>
                                    </table>
                                </td>
                            </tr>
                            </tbody>
                        </table>
                    </td>
                </tr>
                </tbody>
            </table>";
            SendEmailRequest content = new SendEmailRequest();
            StringBuilder htmlBuilder = new StringBuilder();

            foreach (var item in listConsignSaleLine)
            {
                string filledTemplate = consignTemplate
                    .Replace("{PRODUCT_NAME}", item.ProductName)
                    .Replace("{GENDER}", item.Gender.ToString())
                    .Replace("{SIZE}", item.Size.ToString())
                    .Replace("{COLOR}", item.Color)
                    .Replace("{NOTE}", item.Note)
                    .Replace("{RESPONSEFROMSHOP}", item.ResponseFromShop ?? "N/A")
                    .Replace("{Condition}", item.Condition)
                    .Replace("{PRODUCT_IMAGE_URL}",
                        item.Images.Select(c => c.Url).FirstOrDefault())
                    .Replace("{EXPECTED_PRICE}", item.ExpectedPrice.ToString("N0"))
                    .Replace("{DEAL_PRICE}", item.DealPrice!.Value.ToString("N0"));

                htmlBuilder.Append(filledTemplate);
            }

            string finalHtml = htmlBuilder.ToString();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member!.Email;

                var template = GetEmailTemplate("ConsignNegotiatePriceMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Created Date]", consignSale.CreatedDate.AddHours(7).ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.ConsignorName);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[NumberOfNegotiateProduct]", listConsignSaleLine.Count.ToString());
                template = template.Replace("[ConsignTemplate]", finalHtml);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                template = template.Replace("[Status]", "Approved");
                template = template.Replace("[Response]",
                    "Please view your consignment on the website to decide our deal price.");

                content.Subject = $"[GIVEAWAY] CONSIGN NEGOTIATION FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }

            return false;
        }
    }
}