﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

using AutoMapper;
using CovidSafe.API.v20200601.Protos;
using CovidSafe.DAL.Services;
using CovidSafe.Entities.Reports;
using CovidSafe.Entities.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CovidSafe.API.v20200601.Controllers.MessageControllers
{
    /// <summary>
    /// Handles requests to list <see cref="MatchMessage"/> identifiers which are new to a client
    /// </summary>
    [ApiController]
    [ApiVersion("2020-06-01")]
    [Route("api/Messages/[controller]")]
    public class ListController : ControllerBase
    {
        /// <summary>
        /// AutoMapper instance for object resolution
        /// </summary>
        private readonly IMapper _map;
        /// <summary>
        /// <see cref="InfectionReport"/> service layer
        /// </summary>
        private readonly IInfectionReportService _reportService;

        /// <summary>
        /// Creates a new <see cref="ListController"/> instance
        /// </summary>
        /// <param name="map">AutoMapper instance</param>
        /// <param name="reportService"><see cref="InfectionReport"/> service layer</param>
        public ListController(IMapper map, IInfectionReportService reportService)
        {
            // Assign local values
            this._map = map;
            this._reportService = reportService;
        }

        /// <summary>
        /// Get <see cref="MessageInfo"/> for a region, starting at a provided timestamp
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/Messages/List?lastTimestamp=0&amp;api-version=2020-06-01
        ///     
        /// </remarks>
        /// <param name="lastTimestamp">Latest <see cref="MatchMessage"/> timestamp on client device, in ms from UNIX epoch</param>
        /// <param name="cancellationToken">Cancellation token (not required in API call)</param>
        /// <response code="200">Successful request with results</response>
        /// <response code="400">Malformed or invalid request provided</response>
        /// <returns>Collection of <see cref="MessageInfo"/> objects matching request parameters</returns>
        [HttpGet]
        [Produces("application/x-protobuf", "application/json")]
        [ProducesResponseType(typeof(MessageListResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status400BadRequest)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult<MessageListResponse>> GetAsync([Required] long lastTimestamp, CancellationToken cancellationToken = default)
        {
            try
            {
                // Pull queries matching parameters
                IEnumerable<InfectionReportMetadata> results = await this._reportService
                    .GetLatestInfoAsync(lastTimestamp, cancellationToken);

                // Return using mapped proto object
                return Ok(this._map.Map<MessageListResponse>(results));
            }
            catch (RequestValidationFailedException ex)
            {
                return BadRequest(ex.ValidationResult);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Get total size of <see cref="MatchMessage"/> objects for a <see cref="Region"/> based 
        /// on the provided parameters when using application/x-protobuf
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     HEAD /Messages/List?lastTimestamp=0&amp;api-version=2020-06-01
        ///     
        /// </remarks>
        /// <param name="lastTimestamp">Latest <see cref="MatchMessage"/> timestamp on client device, in ms from UNIX epoch</param>
        /// <param name="cancellationToken">Cancellation token (not required in API call)</param>
        /// <response code="200">Successful request</response>
        /// <response code="400">Malformed or invalid request provided</response>
        /// <returns>
        /// Total size of matching <see cref="MatchMessage"/> objects (via Content-Type header), in bytes, based 
        /// on their size when converted to the Protobuf format
        /// </returns>
        [HttpHead]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<ActionResult> HeadAsync([Required] long lastTimestamp, CancellationToken cancellationToken = default)
        {
            try
            {
                // Pull queries matching parameters
                long size = await this._reportService
                    .GetLatestDataSizeAsync(lastTimestamp, cancellationToken);

                // Set Content-Length header with calculated size
                Response.ContentLength = size;

                return Ok();
            }
            catch (RequestValidationFailedException ex)
            {
                return BadRequest(ex.ValidationResult);
            }
            catch (ArgumentNullException)
            {
                return BadRequest();
            }
        }
    }
}