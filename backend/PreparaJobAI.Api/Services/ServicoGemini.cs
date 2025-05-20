using GenerativeAI;

namespace PreparaJobAI.Api.Services
{
    public class ServicoGemini
    {
        private readonly ILogger<ServicoGemini> _logger;
        private readonly GenerativeModel _geminiModel;

        public ServicoGemini(IConfiguration configuration, ILogger<ServicoGemini> logger)
        {
            _logger = logger;
            var apiKey = configuration["GeminiApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("API Key do Gemini não foi encontrada na configuração (appsettings.Development.json).");
                throw new ArgumentNullException(apiKey, "API Key do Gemini é necessária.");
            }

            // Use o nome do modelo como string. Exemplos: "gemini-pro", "gemini-1.5-flash-latest".
            // "gemini-1.5-flash-latest" é uma boa opção para tarefas de texto rápidas e eficientes.
            // O README do pacote gunpal5/Google_GenerativeAI menciona Model.GeminiPro,
            // mas usar a string do nome do modelo é geralmente mais flexível.
            string modelName = "gemini-2.0-flash";
            var googleAI = new GoogleAi(apiKey);

            //_geminiModel = new GenerativeModel(apiKey: apiKey, model: modelName);
            _geminiModel = googleAI.CreateGenerativeModel(modelName);
            _logger.LogInformation("Servico Gemini inicializado com o modelo: {ModelName} usando o pacote gunpal5/Google_GenerativeAI.", modelName);
        }

        public async Task<string> GerarAnaliseTextoSimplesAsync(string promptUsuario, string contexto)
        {
            const int maxContextLength = 100000;
            if (contexto.Length > maxContextLength)
            {
                contexto = contexto.Substring(0, maxContextLength);
                _logger.LogWarning("Contexto truncado para {MaxContextLength} caracteres para evitar exceder limite de tokens.", maxContextLength);
            }
            
            _logger.LogInformation("Enviando prompt para o Gemini. Tamanho do contexto: {ContextoLength}", contexto.Length);

            string promptCompleto = $"{promptUsuario}\n\n--- CONTEXTO ---\n{contexto}\n\n--- FIM CONTEXTO ---";

            try
            {
                var response = await _geminiModel.GenerateContentAsync(promptCompleto);

                // O método Text() é uma forma conveniente de obter a resposta de texto
                string textoGerado = response.Text();
                

                if (string.IsNullOrEmpty(textoGerado))
                {
                    _logger.LogWarning("Gemini (gunpal5/Google_GenerativeAI) retornou uma resposta vazia ou sem texto.");
                    if (response.Candidates != null && response.Candidates.Any())
                    {
                        var firstCandidate = response.Candidates.First();
                        _logger.LogInformation("Gemini FinishReason: {FinishReason}", firstCandidate.FinishReason);
                        if (firstCandidate.SafetyRatings != null && firstCandidate.SafetyRatings.Any())
                        {
                            _logger.LogWarning("Gemini SafetyRatings: {SafetyRatings}",
                                string.Join(", ", firstCandidate.SafetyRatings.Select(r => $"{r.Category}: {r.Probability}")));
                        }
                    }
                    if (response.PromptFeedback != null)
                    {
                        _logger.LogWarning("Gemini PromptFeedback BlockReason: {BlockReason}", response.PromptFeedback.BlockReason);
                    }
                    return "A IA não conseguiu gerar uma análise para este texto no momento.";
                }

                _logger.LogInformation("Resposta recebida do Gemini (gunpal5/Google_GenerativeAI).");
                return textoGerado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao tentar gerar conteúdo com o Gemini (gunpal5/Google_GenerativeAI).");
                return $"Desculpe, ocorreu um erro ao tentar analisar o texto com a IA: {ex.Message}";
            }
        }
    }
}