using System.Text;
using Microsoft.AspNetCore.Mvc;
using PreparaJobAI.Api.Models;
using PreparaJobAI.Api.Services;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace PreparaJobAI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnaliseDocumentosController : ControllerBase
    {
        private readonly ILogger<AnaliseDocumentosController> _logger;
        private readonly ServicoGemini _servicoGemini;

        public AnaliseDocumentosController(ILogger<AnaliseDocumentosController> logger, ServicoGemini servicoGemini)
        {
            _logger = logger;
            _servicoGemini = servicoGemini;
        }

        [HttpPost("curriculo")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroModel), StatusCodes.Status400BadRequest)]
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

                // **CHAMADA REAL AO GEMINI:**
                string promptParaGemini = "Por favor, analise o seguinte texto de um currículo e extraia os principais pontos chave como habilidades, experiências relevantes e formação. Apresente um breve resumo do perfil do candidato.";
                string analiseDoGemini = await _servicoGemini.GerarAnaliseTextoSimplesAsync(promptParaGemini, textoDoPdf);

                var resultadoReal = new
                {
                    mensagem = $"Currículo '{arquivoCurriculo.FileName}' recebido e analisado pela IA!",
                    nomeArquivo = arquivoCurriculo.FileName,
                    previewTextoExtraido = textoDoPdf.Length > 200 ? textoDoPdf[..200] + "..." : textoDoPdf,
                    analiseIA = analiseDoGemini // Resposta direta do Gemini
                };

                return Ok(resultadoReal);
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

        [HttpPost("vaga")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErroModel), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErroModel), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SubmeterVaga([FromBody] VagaModel vagaInput)
        {
            if (!ModelState.IsValid)
            {
                var detalhesErro = ModelState.Values
                                        .SelectMany(v => v.Errors)
                                        .Select(e => e.ErrorMessage)
                                        .ToList();
                _logger.LogWarning("Submissão de vaga com dados inválidos: {Erros}", string.Join("; ", detalhesErro));

                var mensagemErro = $"Dados da vaga inválidos: {string.Join("; ", detalhesErro)}";
                return BadRequest(new ErroModel(mensagemErro, "ERR_004"));
            }

            _logger.LogInformation(
                "Descrição da vaga recebida. Preview: '{Preview}'. Link: {Link}",
                vagaInput.Descricao != null ? vagaInput.Descricao[..Math.Min(vagaInput.Descricao.Length, 100)] + (vagaInput.Descricao.Length > 100 ? "..." : "") : "N/A",
                vagaInput.Link ?? "N/A"
            );

            try
            {
                _logger.LogInformation("Enviando descrição da vaga para análise do Gemini...");

                string promptParaGeminiVaga = "Por favor, analise a seguinte vaga de emprego. Extraia os principais requisitos (habilidades técnicas, experiência necessária, formação), as responsabilidades do cargo e, se possível, identifique aspectos da cultura da empresa mencionada ou implícita no link ou na descrição da vaga.";

                var contexto = $"Link: {vagaInput.Link}\n\nDescrição: {vagaInput.Descricao}";

                // O contexto aqui é a própria descrição da vaga
                string analiseDaVagaPeloGemini = await _servicoGemini.GerarAnaliseTextoSimplesAsync(promptParaGeminiVaga, contexto);

                var resultadoReal = new
                {
                    mensagem = "Descrição da vaga recebida e analisada pela IA!",
                    linkFornecido = vagaInput.Link,
                    analiseIA = analiseDaVagaPeloGemini
                };

                return Ok(resultadoReal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar a submissão da descrição da vaga.");
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new ErroModel(
                        "Ocorreu um erro interno ao processar a descrição da vaga. Tente novamente mais tarde.",
                        StatusCodes.Status500InternalServerError.ToString()
                    )
                );
            }
        }
    }
}
