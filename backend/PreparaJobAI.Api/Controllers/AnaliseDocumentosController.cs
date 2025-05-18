using Microsoft.AspNetCore.Mvc;
using PreparaJobAI.Api.Models;

namespace PreparaJobAI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnaliseDocumentosController : ControllerBase
    {
        private readonly ILogger<AnaliseDocumentosController> _logger;

        public AnaliseDocumentosController(ILogger<AnaliseDocumentosController> logger)
        {
            _logger = logger;
        }

        // POST: api/analisedocumentos/curriculo
        [HttpPost("curriculo")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)] // Exemplo de tipo de retorno para Swagger
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult UploadCurriculo(IFormFile arquivoCurriculo)
        {
            if (arquivoCurriculo == null || arquivoCurriculo.Length == 0)
            {
                _logger.LogWarning("Tentativa de upload de currículo sem arquivo ou com arquivo vazio.");
                return BadRequest("Nenhum arquivo foi enviado ou o arquivo está vazio.");
            }

            // Opcional, mas bom para o MVP: Logar o tipo de conteúdo, mas não bloquear se não for PDF ainda.
            _logger.LogInformation(
                "Currículo recebido: {nomeArquivo}, Tamanho: {tamanhoArquivo} bytes, Tipo: {tipoArquivo}",
                arquivoCurriculo.FileName,
                arquivoCurriculo.Length,
                arquivoCurriculo.ContentType
            );

            if (arquivoCurriculo.ContentType != "application/pdf")
            {
                _logger.LogWarning(
                    "Formato de arquivo não usual para currículo: {tipo}. Nome: {nomeArqwuivo}. Processando mesmo assim para o MVP.",
                    arquivoCurriculo.ContentType,
                    arquivoCurriculo.FileName
                );

                return BadRequest(new ErroModel("Formato de arquivo inválido. Por favor, envie um PDF.", "ERR_001"));
            }

            try
            {
                _logger.LogInformation(
                    "Iniciando processamento do currículo: {nomeArquivo}",
                    arquivoCurriculo.FileName
                );

                // **LÓGICA DE PROCESSAMENTO DO CURRÍCULO (PLATZHALTER) **
                // Aqui é onde você irá:
                // 1. Extrair o texto do PDF.
                //    - Você precisará de uma biblioteca para isso (ex: PdfPig).
                //    - Exemplo conceitual (NÃO FUNCIONAL AINDA):
                //      string textoDoPdf;
                //      using (var stream = arquivoCurriculo.OpenReadStream())
                //      {
                //          // textoDoPdf = SuaLogicaDeExtrairTextoComPdfPig(stream);
                //      }
                //      _logger.LogInformation($"Texto extraído (simulado) para: {arquivoCurriculo.FileName}");

                // 2. Enviar o texto extraído para o seu Agente Gemini que lê currículos.
                //    - Exemplo conceitual:
                //      // var analiseGemini = await _servicoGemini.AnalisarCurriculoAsync(textoDoPdf);
                //      _logger.LogInformation($"Análise com Gemini (simulada) para: {arquivoCurriculo.FileName}");


                // **RESPOSTA SIMULADA PARA O MVP (enquanto a lógica do Gemini não está pronta):**
                var resultadoSimulado = new
                {
                    mensagem = $"Currículo '{arquivoCurriculo.FileName}' recebido e processamento simulado com sucesso!",
                    pontosChaveExtraidos = new[] { "Desenvolvedor C# .NET (simulado)", "Experiência com APIs REST (simulado)", "Banco de Dados SQL Server (simulado)" },
                    resumoIA = "Este é um resumo simulado gerado pela IA sobre o currículo enviado."
                };

                return Ok(resultadoSimulado);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao processar o arquivo de currículo: {nomeArquivo}",
                    arquivoCurriculo.FileName
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    "Ocorreu um erro interno ao processar o seu currículo. Tente novamente mais tarde."
                );
            }
        }
    }
}