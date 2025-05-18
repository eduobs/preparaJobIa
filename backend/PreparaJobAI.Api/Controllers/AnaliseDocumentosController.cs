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

                return BadRequest(new ErroModel($"Dados da vaga inválidos. {detalhesErro}", "ERR_004"));
            }

            _logger.LogInformation(
                "Descrição da vaga recebida. Preview: '{preview}{descricao}'. Link: {link}",
                vagaInput.Descricao[..Math.Min(vagaInput.Descricao.Length, 100)],
                vagaInput.Descricao.Length > 100 ? "..." : "",
                vagaInput.Link ?? "N/A"
            );

            try
            {
                // **LÓGICA DE PROCESSAMENTO DA DESCRIÇÃO DA VAGA (PLATZHALTER) **
                // Aqui você irá:
                // 1. (Opcional) Salvar a descrição da vaga e o link se for relevante para o fluxo.
                // 2. Preparar esta descrição para ser enviada ao Agente Gemini que analisa vagas.
                //    Exemplo conceitual:
                //    // var analiseVagaGemini = await _servicoGemini.AnalisarVagaAsync(vagaInput.Descricao, vagaInput.Link);
                _logger.LogInformation("Análise da vaga com Gemini (simulada).");


                // **RESPOSTA SIMULADA PARA O MVP:**
                var resultadoSimulado = new
                {
                    mensagem = "Descrição da vaga recebida e processamento simulado com sucesso!",
                    // Você pode retornar um ID se armazenar a vaga, ou um resumo simulado da IA
                    resumoRequisitosIA = new[] { "Requisito X (simulado)", "Habilidade Y (simulada)", "Experiência Z (simulada)" },
                    culturaEmpresaIA = "Cultura da empresa parece ser focada em colaboração e inovação (simulado)."
                };

                // Simula um pequeno delay como se estivesse processando
                await Task.Delay(100); // Apenas para simulação, remova em produção

                return Ok(resultadoSimulado);
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
