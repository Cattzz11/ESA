using Azure;
using Square.Exceptions;
using Square;
using Square.Http.Client;
using System.Threading.Tasks;
using Square.Models;
using PROJETOESA.Services;
using PROJETOESA.Data;

namespace PROJETOESA.Services
{
    public class SquareService
    {
        private SquareClient _client;
        private AeroHelperContext _context;
        public SquareService(SquareClient client, AeroHelperContext context)
        {
            _client = client ?? throw new ArgumentNullException(nameof(_client));
            _context = context;
        }

        public async Task PayAsync(Models.PaymentModel payment)
        {
            long lPrice = (long)payment.price;
            var amountMoney = new Money.Builder()
                .Amount(lPrice)
                .Currency(payment.currency)
                .Build();

            var body = new CreatePaymentRequest.Builder(sourceId: "cnon:card-nonce-ok", idempotencyKey: "ff1dde35-032c-4a30-bb9e-b586f2963018", amountMoney)
              .AmountMoney(amountMoney)
              .Build();

            try
            {
                // ADICIONAR DADOS NA BD PARA HISTORICO
                //Criar Customer, criar Order e no fim Criar pagamento.
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
        
    
