using Azure;
using Square.Exceptions;
using Square;
using Square.Http.Client;
using System.Threading.Tasks;
using Square.Models;
using PROJETOESA.Services;

namespace PROJETOESA.Services
{
    public class SquareService
    {
        private SquareClient _client;
        public SquareService(SquareClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(_client));
        }

        public async Task PayAsync(Models.PaymentModel payment)
        {
            var amountMoney = new Money.Builder()
                .Amount(payment.price)
                .Currency(payment.currency)
                .Build();

            var body = new CreatePaymentRequest.Builder(sourceId: "cnon:card-nonce-ok", idempotencyKey: "ff1dde35-032c-4a30-bb9e-b586f2963018", amountMoney)
              .AmountMoney(amountMoney)
              .Build();

            try
            {
                // var result = await client.PaymentsApi.CreatePaymentAsync(body: body);
                var result = await _client.PaymentsApi.CreatePaymentAsync(body);
            }
            catch (ApiException e)
            {
                Console.WriteLine("Failed to make the request");
                Console.WriteLine($"Response Code: {e.ResponseCode}");
                Console.WriteLine($"Exception: {e.Message}");
            }
        }
    }

}
        
    
