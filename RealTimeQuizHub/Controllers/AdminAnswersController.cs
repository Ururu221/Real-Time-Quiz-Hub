using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services;
using RealTimeQuizHub.Services.Interfaces;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/admin/answers")]
    //[Authorize(Policy = "AdminOnly")]
    public class AdminAnswersController : ControllerBase
    {
        private readonly IAnswerService _answerService;
        private readonly IQuestionService _questionService;
        public AdminAnswersController(IAnswerService answerService, IQuestionService questionService)
        {
            _answerService = answerService;
            _questionService = questionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? questionId)
        {
            try
            {
                List<Answer> result;
                if (questionId.HasValue)
                {
                    result = await _answerService.GetAnswersByQuestionIdAsync(questionId.Value);
                }
                else
                {
                    result = await _answerService.GetAllAnswersAsync();
                }
                return Ok(result);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Помилка сервера");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var answer = await _answerService.GetAnswerByIdAsync(id);
                if (answer == null)
                {
                    return NotFound("Відповідь не знайдено");
                }
                return Ok(answer);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Помилка сервера");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAnswerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var question = await _questionService.GetQuestionByIdAsync(dto.QuestionId);
                if (question == null)
                {
                    return NotFound("Не знайдено відповіді на це запитання");
                }
                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return BadRequest("Текст відповіді не може бути порожнім");
                }

                var answer = new Answer
                {
                    QuestionId = dto.QuestionId,
                    Text = dto.Text,
                    IsCorrect = dto.IsCorrect
                };

                var check = await _answerService.AddAnswerAsync(answer);
                return Ok(check);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Помилка сервера");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAnswerDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return BadRequest("Текст відповіді не може бути порожнім");
                }
                var existing = _answerService.GetAnswerByIdAsync(id);
                if (existing == null)
                {
                    return NotFound("Відповідь не знайдено");
                }

                var answer = new Answer
                {
                    Id = id,
                    Text = dto.Text,
                    IsCorrect = dto.IsCorrect
                };

                await _answerService.UpdateAnswerAsync(answer);
                return Ok("Відповідь оновлено");
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Помилка сервера");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool success = await _answerService.DeleteAnswerAsync(id);
                if (!success)
                {
                    return NotFound("Відповідь не знайдено");
                }
                return NoContent();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Помилка сервера");
            }
        }
    }
}
