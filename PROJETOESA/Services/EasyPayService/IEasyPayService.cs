using PROJETOESA.Models;
using PROJETOESA.Services.EasyPayService.Models;

namespace PROJETOESA.Services.EasyPayService
{
    public interface IEasyPayService
    {
        /// <summary>
        /// Método que permite gerar um single payment da API EasyPay (Multibanco ou MB WAY).
        /// </summary>
        /// <param name="limitDate">Data limite do pagamento.</param>
        /// <param name="value">Valor do pagamento.</param>
        /// <param name="email">E-mail do utilizador associado ao empréstimo.</param>
        /// <returns>
        /// Retorna um objeto da classe Payment com todos os dados necessários para o pagamento.
        /// </returns>
        Task<Payment> CreateSinglePayment(DateTime limitDate, ApplicationUser user, double value, string method);

        /// <summary>
        /// Método que permite obter os detalhes de um single payment da API EasyPay.
        /// </summary>
        /// <param name="id">ID do single payment.</param>
        /// <returns>
        /// Retorna um objeto da classe SinglePaymentDetails com todos os dados necessários.
        /// </returns>
        Task<SinglePaymentDetails> ShowSinglePaymentDetails(Guid id);

    }
}
