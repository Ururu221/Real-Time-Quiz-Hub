using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealTimeQuizHub.Models.Dtos;
using RealTimeQuizHub.Models;
using RealTimeQuizHub.Services;

namespace RealTimeQuizHub.Controllers
{
    [ApiController]
    [Route("api/admin/answers")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminAnswersController : ControllerBase
    {
        private readonly AnswerService _answerService;
        private readonly QuestionService _questionService;
        public AdminAnswersController(AnswerService answerService, QuestionService questionService)
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
                return StatusCode(500, "Ошибка сервера");
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
                    return NotFound("Ответ не найден");
                }
                return Ok(answer);
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
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
                // Проверяем, существует ли вопрос, к которому добавляется ответ
                var question = await _questionService.GetQuestionByIdAsync(dto.QuestionId);
                if (question == null)
                {
                    return NotFound("Вопрос для ответа не найден");
                }
                if (string.IsNullOrWhiteSpace(dto.Text))
                {
                    return BadRequest("Текст ответа не может быть пустым");
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
                return StatusCode(500, "Ошибка сервера");
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
                    return BadRequest("Текст ответа не может быть пустым");
                }
                var existing = _answerService.GetAnswerByIdAsync(id);
                if (existing == null)
                {
                    return NotFound("Ответ не найден");
                }

                var answer = new Answer
                {
                    Id = id,
                    Text = dto.Text,
                    IsCorrect = dto.IsCorrect
                };

                await _answerService.UpdateAnswerAsync(answer);
                return Ok("Ответ обновлен");
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
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
                    return NotFound("Ответ не найден");
                }
                return NoContent();
            }
            catch (System.Exception)
            {
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
