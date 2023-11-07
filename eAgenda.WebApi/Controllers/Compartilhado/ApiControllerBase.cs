﻿using eAgenda.Aplicacao.Compartilhado;
using eAgenda.Dominio.Compartilhado;
using Microsoft.AspNetCore.Mvc;

namespace eAgenda.WebApi.Controllers.Compartilhado
{
    public abstract class ApiControllerBase<TFormViewModel, TListarViewMode, TVisualizarViewModel, TEntidade, TControlador> : ControllerBase
        where TFormViewModel : ViewModelBase<TFormViewModel>
        where TListarViewMode : ViewModelBase<TListarViewMode>
        where TVisualizarViewModel : ViewModelBase<TVisualizarViewModel>
        where TEntidade : EntidadeBase<TEntidade>
        where TControlador : class
    {
        protected readonly ILogger<TControlador> _logger;
        protected readonly IMapper _mapeador;

        protected IServicoApiBase<TEntidade> _servico;
        protected string entidade = "";

        public ApiControllerBase(IServicoApiBase<TEntidade> servico, IMapper mapeador, ILogger<TControlador> logger)
        {
            _servico = servico;
            _mapeador = mapeador;
            _logger = logger;
        }

        [HttpGet]
        //[ProducesResponseType(typeof(ListarContatoViewModel), 200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult SelecionarTodos()
        {
            this._logger.LogInformation($"Selecionando todos os registos de {entidade}s");

            List<TEntidade> registro = _servico.SelecionarTodos().Value;

            return Ok(new
            {
                Sucesso = true,
                Dados = _mapeador.Map<List<TListarViewMode>>(registro),
                QtdRegistros = registro.Count
            });
        }

        [HttpGet("{id}")]
        //[ProducesResponseType(typeof(VisualizarContatoViewModel), 200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string[]), 404)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult SelecionarPorId(Guid id)
        {
            this._logger.LogInformation($"Selecionando {entidade} com o id: {id}");

            Result<TEntidade> resultadoBusca = _servico.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
                return NotFound(new
                {
                    Sucesso = false,
                    Erros = string.Join("\r\n", resultadoBusca.Errors.Select(e => e.Message).ToArray())
                });

            return Ok(new
            {
                Sucesso = true,
                Dados = _mapeador.Map<TFormViewModel>(resultadoBusca)
            });
        }

        [HttpGet("visualizacao-completa/{id}")]
        //[ProducesResponseType(typeof(VisualizarContatoViewModel), 200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string[]), 404)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult SelecionarCompletoPorId(Guid id)
        {
            this._logger.LogInformation($"Selecionando todas as informações de {entidade} com o id: {id}");

            Result<TEntidade> resultadoBusca = _servico.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
                return NotFound(new
                {
                    Sucesso = false,
                    Erros = string.Join("\r\n", resultadoBusca.Errors.Select(e => e.Message).ToArray())
                });

            return Ok(new
            {
                Sucesso = true,
                Dados = _mapeador.Map<TVisualizarViewModel>(resultadoBusca)
            });
        }

        [HttpPost]
        //[ProducesResponseType(typeof(FormContatoViewModel), 201)]
        [ProducesResponseType(201)]
        [ProducesResponseType(typeof(string[]), 400)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult Inserir(TFormViewModel formViewModel)
        {
            this._logger.LogInformation($"Inserindo {entidade}");

            TEntidade registro = _mapeador.Map<TEntidade>(formViewModel);

            return ProcessarResultado(_servico.Inserir(registro), formViewModel);
        }

        [HttpPut("{id}")]
        //[ProducesResponseType(typeof(FormContatoViewModel), 200)]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string[]), 400)]
        [ProducesResponseType(typeof(string[]), 404)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult Editar(Guid id, TFormViewModel formViewModel)
        {
            this._logger.LogInformation($"Editando {entidade}");

            Result<TEntidade> resultadoBusca = _servico.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
                return NotFound(new
                {
                    Sucesso = false,
                    Erros = string.Join("\r\n", resultadoBusca.Errors.Select(e => e.Message).ToArray())
                });

            TEntidade registro = _mapeador.Map(formViewModel, resultadoBusca.Value);

            return ProcessarResultado(_servico.Editar(registro), formViewModel);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(string[]), 400)]
        [ProducesResponseType(typeof(string[]), 404)]
        [ProducesResponseType(typeof(string[]), 500)]
        public IActionResult Excluir(Guid id)
        {
            this._logger.LogInformation($"Excluindo {entidade}");

            Result<TEntidade> resultadoBusca = _servico.SelecionarPorId(id);

            if (resultadoBusca.IsFailed)
                return NotFound(new
                {
                    Sucesso = false,
                    Erros = string.Join("\r\n", resultadoBusca.Errors.Select(e => e.Message).ToArray())
                });

            return ProcessarResultado(_servico.Excluir(resultadoBusca.Value));
        }

        private IActionResult ProcessarResultado(Result<TEntidade> resultadoBusca, TFormViewModel formViewModel = null)
        {
            if (resultadoBusca.IsFailed)
                return BadRequest(new
                {
                    sucesso = false,
                    erros = resultadoBusca.Errors.Select(e => e.Message)
                });

            return Ok(new
            {
                Sucesso = true,
                Dados = formViewModel
            });
        }
    }
}
