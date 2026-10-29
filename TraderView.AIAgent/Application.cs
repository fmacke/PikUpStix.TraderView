using Mscc.GenerativeAI;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Infrastructure.DbContexts;
using TraderView.Infrastructure.Repositories;
using Mscc.GenerativeAI.Types;

namespace TraderView.AIAgent
{
    public class Application
    {
        private readonly IAIReportService aiReportService;
        private readonly IAIReportInstructionService instructionService;
        private readonly IPositionService positionService;
        private readonly IConfiguration config;

        public Application(IAIReportService aiReportService, IAIReportInstructionService instructionService, IConfiguration config, IPositionService positionService)
        {
            this.aiReportService = aiReportService;
            this.instructionService = instructionService;
            this.config = config;
            this.positionService = positionService;
        }
        public async Task RunAsync()
        {
            var instruction = await instructionService.GetByIdAsync(2);// Assuming the instruction with Id = 2 is the one we want to use
            var positions = await GetOpenPositionsString();
            var prompt = instruction.Instruction.Replace("[INSERT_POSITION_LIST]", positions);
            var apiKey = config["GoogleAIStudio:ApiKey"];
            var googleAI = new GoogleAI(apiKey);

            // Using the cost-effective and fast Flash model for daily queries
            var model = googleAI.GenerativeModel(model: config["GoogleAIStudio:Model"]);
            Console.WriteLine("Fetching morning market data from Gemini...");

            var response = await model.GenerateContent(prompt);

            Console.WriteLine("\n--- MORNING REPORT ---");
            Console.WriteLine(response.Text);

            // Save the response to database using IAIReportService
            var reportId = await aiReportService.CreateAsync(instruction.Id, response.Text);
            Console.WriteLine($"\nReport saved to database with ID: {reportId}");
        }

        private async Task<string> GetOpenPositionsString()
        {
            var positions = await positionService.GetOpenPositionsAsync(); // Assuming this method returns a list of positions
            if(positions.Any())
            {
               return string.Join("; ", positions.Select(p => $"{p.Instrument.ListingExchange}:{p.Instrument.DataName}"));
            }
            return "NO OPEN POSITIONS";
        }
    }
}
