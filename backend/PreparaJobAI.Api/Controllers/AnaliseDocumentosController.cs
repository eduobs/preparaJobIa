using System.Text;
using Microsoft.AspNetCore.Mvc;
using PreparaJobAI.Api.Models;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

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
        public async Task<IActionResult> UploadCurriculoAsync(IFormFile arquivoCurriculo)
        {
            if (arquivoCurriculo == null || arquivoCurriculo.Length == 0)
            {
                _logger.LogWarning("Tentativa de upload de currículo sem arquivo ou com arquivo vazio.");
                return BadRequest(new ErroModel("Nenhum arquivo foi enviado ou o arquivo está vazio.", "ERR_001"));
            }

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

                return BadRequest(new ErroModel("Formato de arquivo inválido. Por favor, envie um PDF.", "ERR_002"));
            }
            
            string textoDoPdf = string.Empty;

            try
            {
                _logger.LogInformation(
                    "Iniciando processamento do currículo: {nomeArquivo}",
                    arquivoCurriculo.FileName
                );

                // Usar MemoryStream é uma boa prática para IFormFile, pois evita salvar em disco desnecessariamente.
                using (var memoryStream = new MemoryStream())
                {
                    await arquivoCurriculo.CopyToAsync(memoryStream);
                    memoryStream.Position = 0; // Resetar a posição do stream para o início

                    using (PdfDocument document = PdfDocument.Open(memoryStream))
                    {
                        var textoCompleto = new StringBuilder();
                        foreach (Page page in document.GetPages())
                        {
                            textoCompleto.Append(page.Text);
                            textoCompleto.Append(" "); // Adiciona um espaço entre o texto de diferentes páginas
                        }
                        textoDoPdf = textoCompleto.ToString();
                    }
                }

                if (string.IsNullOrWhiteSpace(textoDoPdf))
                {
                    _logger.LogWarning(
                        "Não foi possível extrair texto do PDF: {nomeArquivo} ou o PDF não contém texto.",
                        arquivoCurriculo.FileName
                    );

                    return BadRequest(new ErroModel("Não foi possível extrair texto do PDF.", "ERR_003"));
                }
                else
                {
                    _logger.LogInformation(
                        "Texto extraído com sucesso do PDF: {nomeArquivo}. Tamanho do texto: {tamnhoTexto} caracteres.",
                        arquivoCurriculo.FileName,
                        textoDoPdf.Length
                    );
                }

                // **LÓGICA DE ENVIO PARA O GEMINI (PLATZHALTER) **
                // Agora você tem `textoDoPdf`. O próximo passo seria enviar este texto para o Gemini.
                // var analiseGemini = await _servicoGemini.AnalisarCurriculoAsync(textoDoPdf);

                // **RESPOSTA SIMULADA PARA O MVP (com o texto extraído incluído):**
                var resultadoSimulado = new
                {
                    mensagem = $"Currículo '{arquivoCurriculo.FileName}' recebido e texto extraído com sucesso!",
                    nomeArquivo = arquivoCurriculo.FileName,
                    // Preview do texto extraído (primeiros 500 caracteres, por exemplo)
                    previewTextoExtraido = textoDoPdf.Length > 500 ? textoDoPdf.Substring(0, 500) + "..." : textoDoPdf,
                    // A análise abaixo ainda é simulada, mas usaria o textoDoPdf para chamar o Gemini
                    analiseIA = new
                    {
                        pontosChave = new[] { "Habilidade A (baseada no texto)", "Experiência B (baseada no texto)" },
                        resumo = "Resumo do currículo gerado pela IA (baseado no texto)."
                    }
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