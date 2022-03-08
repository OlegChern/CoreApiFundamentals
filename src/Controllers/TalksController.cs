using System;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly LinkGenerator _linkGenerator;
        private readonly IMapper _mapper;

        public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _linkGenerator = linkGenerator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> Get(string moniker)
        {
            try
            {
                var talks = await _campRepository.GetTalksByMonikerAsync(moniker, true);
                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get the talks");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> Get(string moniker, int id)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get the requested talk");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel talkModel)
        {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker);
                if (camp == null)
                {
                    return BadRequest("Camp does not exist");
                }

                var talk = _mapper.Map<Talk>(talkModel);
                talk.Camp = camp;

                if (talkModel.Speaker == null)
                {
                    return BadRequest("Speaker ID is required");
                }

                var speaker = await _campRepository.GetSpeakerAsync(talkModel.Speaker.SpeakerId);
                if (speaker == null)
                {
                    return BadRequest("Speaker could not be found");
                }

                talk.Speaker = speaker;

                _campRepository.Add(talk);
                if (await _campRepository.SaveChangesAsync())
                {
                    var url = _linkGenerator.GetPathByAction(HttpContext, "Get", values: new { moniker, id = talk.TalkId });
                    
                    return Created(url, _mapper.Map<TalkModel>(talk));
                }
                else
                {
                    return BadRequest("Failed to save new Talk");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get the talks");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> Put(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                _mapper.Map(model, talk);

                if (model.Speaker != null)
                {
                    var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                if (await _campRepository.SaveChangesAsync())
                {
                    return _mapper.Map<TalkModel>(talk);
                }
                else
                {
                    return BadRequest("Failed to update database");
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get the requested talk");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(string moniker, int id)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null)
                {
                    return NotFound("Couldn't find the talk");
                }

                _campRepository.Delete(talk);
                if (await _campRepository.SaveChangesAsync())
                {
                    return Ok();
                }
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }

            return BadRequest("Failed to delete the talk");
        }
    }
}
