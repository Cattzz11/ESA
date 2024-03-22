using PROJETOESA.Models;
using PROJETOESA.Services.EasyPayService.Models;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace PROJETOESA.Services.EasyPayService
{
    public class EasyPayService
    {
        public async Task<Payment> CreateSinglePayment(DateTime limitDate, ApplicationUser user, double value, string method)
        {
            var client = new HttpClient();

            var formattedDate = new DateTime(limitDate.Year, limitDate.Month, limitDate.Day, limitDate.Hour, limitDate.Minute, 0);

            RequestData requestData = new RequestData();

            if (method.Equals("mbw"))
            {
                requestData.ExpirationTime = formattedDate;

                CustomerData customerData = new CustomerData()
                {
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.PhoneNumber
                };

                requestData.Customer = customerData;
                requestData.Value = value;
                requestData.Method = method;
            }
            else
            {
                requestData.ExpirationTime = formattedDate;

                CustomerData customerData = new CustomerData()
                {
                    Name = null,
                    Email = user.Email,
                    Phone = null
                };

                requestData.Customer = customerData;
                requestData.Value = value;
                requestData.Method = method;
            }

            var json = JsonSerializer.Serialize(requestData);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("single", content);

            Payment payment = new Payment();

            if(response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonNode.Parse(responseContent);

                payment.PaymentId = Guid.Parse((string)data["id"]);
                payment.Price = value;
                payment.Entity = method.Equals("mb") ? int.Parse((string)data["method"]["entity"]) : null;
                payment.Reference = method.Equals("mb") ? int.Parse((string)data["method"]["reference"]) : null;
                payment.LimitDate = formattedDate;
                payment.paymentStatus = PaymentStatus.InProgress;
                payment.PaymentMethod = method.Equals("mbw")?"MBW":"MB";
            }
            return payment;
        }

        public async Task<SinglePaymentDetails> ShowSinglePaymentDetails(Guid id)
        {
            var client = new HttpClient();

            var response = await client.GetAsync($"single{id}");

            SinglePaymentDetails paymentDetails = new SinglePaymentDetails();

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = JsonNode.Parse(responseContent);

                MethodData method = new MethodData();
                method.Type = (string)data["method"]["type"];
                method.Entity = method.Type.ToLower().Equals("mb") ? int.Parse((string)data["method"]["entity"]):null;
                method.Reference = method.Equals("mb") ? int.Parse((string)data["method"]["reference"]) : null;

                CustomerData customer = new CustomerData();
                customer.Email = (string)data["customer"]["email"];
                customer.Name = method.Type.ToLower().Equals("mbw") ? (string)data["customer"]["name"] : null;
                customer.Phone = method.Type.ToLower().Equals("mbw") ? (string)data["customer"]["phone"] : null;

                paymentDetails.Id = Guid.Parse((string)data["id"]);
                paymentDetails.Customer = customer;
                paymentDetails.Value = (double)data["value"];
                paymentDetails.Method = method;
                paymentDetails.PaymentStatus = (string)data["payment_status"];
                paymentDetails.PaidAt = DateTime.Parse((string)data["paid_at"]);

            }
            return paymentDetails;
        }
        
    }
}
