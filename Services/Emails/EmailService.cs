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
using Dao;
using DotNext;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Bcpg.Attr;
using Repositories.ConsignSales;
using Repositories.Orders;

namespace Services.Emails
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountRepository _accountRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IConsignSaleRepository _consignSaleRepository;

        public EmailService(IConfiguration configuration, IAccountRepository accountRepository,
            IOrderRepository orderRepository,
            IConsignSaleRepository consignSaleRepository)
        {
            _configuration = configuration;
            _accountRepository = accountRepository;
            _orderRepository = orderRepository;
            _consignSaleRepository = consignSaleRepository;
        }

        public string GetEmailTemplate(string templateName)
        {
            string pathTon = Path.Combine("D:\\Captstone\\GiveAway-API\\Services\\MailTemplate\\",
                $"{templateName}.html");
            // string pathLocal = Path.Combine("C:\\FPT_University_FULL\\CAPSTONE_API\\Services\\MailTemplate\\", $"{templateName}.html");*/
            string path = Path.Combine(_configuration.GetSection("EmailTemplateDirectory").Value,
                $"{templateName}.html");
            var template = File.ReadAllText(path, Encoding.UTF8);
            template = template.Replace("[path]", _configuration.GetSection("RedirectDirectory").Value);
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
            response.Messages = ["Register successfully! Please check your email for verification in 3 minutes"];
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
                        .Replace("{PRODUCT_IMAGE_URL}", item.IndividualFashionItem.Images.Select(c => c.Url).First())
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
                template = template.Replace($"[Shipping Fee]", order.ShippingFee.ToString("N0"));
                template = template.Replace($"[Discount]", order.Discount.ToString("N0"));
                template = template.Replace($"[Payment Date]",
                    order.OrderLineItems.Select(c => c.PaymentDate).First().ToString());
                content.Subject = $"[GIVEAWAY] ORDER INVOICE FROM GIVEAWAY";
                content.Body = template;
                await SendEmail(content);
                return true;
            }
            return false;
        }

        public async Task<bool> SendEmailRefund(RefundResponse request)
        {
            SendEmailRequest content = new SendEmailRequest();
            var order = await _orderRepository.GetSingleOrder(c =>
                c.OrderLineItems.Select(c => c.OrderLineItemId).Contains(request.OrderLineItemId));
            var template = GetEmailTemplate("RefundMail");
            template = template.Replace("[Order Code]", order!.OrderCode);
            template = template.Replace("[Status]", request.RefundStatus.ToString());
            template = template.Replace("[Product Name]", request.ItemName);
            template = template.Replace("[Created Date]", request.CreatedDate.ToString("G"));
            template = template.Replace("[Refund Percent]", request.RefundPercentage!.Value.ToString());
            template = template.Replace("[Refund Amount]", request.RefundAmount!.Value.ToString("N0"));
            template = template.Replace("[Customer Name]", request.CustomerName);
            template = template.Replace("[Phone Number]", request.CustomerPhone);
            template = template.Replace("[Email]", request.CustomerEmail);
            template = template.Replace("[Description]", request.Description);
            template = template.Replace("[Response]", request.ResponseFromShop);
            if (order.MemberId != null)
            {
                var member = await GenericDao<Account>.Instance.GetQueryable().Where(c => c.AccountId == order.MemberId)
                    .FirstOrDefaultAsync();
                content.To = member!.Email;
                content.Subject = $"[GIVEAWAY] REFUND RESPONSE FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }

            return false;
        }

        public async Task<bool> SendEmailConsignSale(Guid consignSaleId)
        {
            Expression<Func<ConsignSale, bool>> predicate = consignSale => consignSale.ConsignSaleId == consignSaleId;
            var consignSale = await _consignSaleRepository.GetSingleConsignSale(predicate);
            List<ConsignSaleLineItem> listConsignSaleLine = consignSale!.ConsignSaleLineItems.ToList();
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

            foreach (var item in listConsignSaleLine)
            {
                string filledTemplate = consignTemplate
                    .Replace("{PRODUCT_NAME}", item.ProductName)
                    .Replace("{GENDER}", item.Gender.ToString())
                    .Replace("{SIZE}", item.Size.ToString())
                    .Replace("{COLOR}", item.Color)
                    .Replace("{NOTE}", item.Note)
                    .Replace("{Condition}", item.Condition)
                    .Replace("{PRODUCT_IMAGE_URL}", item.IndividualFashionItem.Images.Select(c => c.Url).First())
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
                template = template.Replace("[Created Date]", consignSale.CreatedDate.ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.ConsignorName);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[ConsignTemplate]", finalHtml);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
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

                content.Subject = $"[GIVEAWAY] CONSIGN SALE ANNOUNCEMENT FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
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

        public async Task<bool> SendEmailConsignSaleReceived(Guid consignId)
        {
            var consignSale = await _consignSaleRepository.GetConsignSaleById(consignId);
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member!.Email;

                var template = GetEmailTemplate("ConsignSaleReceivedMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Start Date]", consignSale.StartDate.GetValueOrDefault().ToString("G"));
                template = template.Replace("[Customer Name]", consignSale.Consginer);
                template = template.Replace("[Phone Number]", consignSale.Phone);
                template = template.Replace("[Email]", consignSale.Email);
                template = template.Replace("[Address]", consignSale.Address);
                template = template.Replace("[Response]",
                    "Thank you for trusting and using the consignment service at Give Away store.");
                template = template.Replace("[End Date]", consignSale.EndDate.GetValueOrDefault().ToString("G"));

                content.Subject = $"[GIVEAWAY] RECEIVED CONSIGNSALE FROM GIVEAWAY";
                content.Body = template;

                await SendEmail(content);
                return true;
            }

            return false;
        }

        public async Task<bool> SendEmailConsignSaleEndedMail(Guid consignId)
        {
            var consignSale = await _consignSaleRepository.GetConsignSaleById(consignId);
            SendEmailRequest content = new SendEmailRequest();
            if (consignSale.MemberId != null)
            {
                var member = await _accountRepository.GetAccountById(consignSale.MemberId.Value);
                content.To = member!.Email;

                var template = GetEmailTemplate("ConsignSaleEndedMail");
                template = template.Replace("[ConsignSale Code]", consignSale.ConsignSaleCode);
                template = template.Replace("[Type]", consignSale.Type.ToString());
                template = template.Replace("[Total Price]", consignSale.TotalPrice.ToString("N0"));
                template = template.Replace("[Sold Price]", consignSale.SoldPrice.ToString("N0"));
                template = template.Replace("[Amount Receive]", consignSale.MemberReceivedAmount.ToString("N0"));
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
    }
}