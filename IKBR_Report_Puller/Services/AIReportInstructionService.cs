using System;
using System.Collections.Generic;
using System.Text;
using TraderView.Application.Interfaces.Repositories;
using TraderView.Application.Interfaces.Services;
using TraderView.Domain.Entities;

namespace TraderView.Application.Services
{
    public class AIReportInstructionService : IAIReportInstructionService
    {
        private readonly IAIReportInstructionRepository instructionRepository;

        public AIReportInstructionService(IAIReportInstructionRepository instructionRepository)
        {
            this.instructionRepository = instructionRepository;
        }
        public async Task<int> CreateAsync(AIReportInstruction instruction)
        {
            var inserted = await Task.Run(() => instructionRepository.AddAsync(instruction));
            return inserted.Id;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            Task deleted = Task.Run(() => instructionRepository.DeleteAsync(new AIReportInstruction { Id = id }));
            return deleted.IsCompleted;
        }

        public async Task<IReadOnlyList<AIReportInstruction>> GetAllAsync()
        {
            return await Task.Run(() => instructionRepository.GetAllAsync());
        }

        public async Task<AIReportInstruction?> GetByIdAsync(int id)
        {
            return await Task.Run(() => instructionRepository.GetByIdAsync(id));
        }

        public async Task<bool> UpdateAsync(int id, string instruction)
        {
            Task updated = Task.Run(() => instructionRepository.UpdateAsync(new AIReportInstruction { Id = id, Instruction = instruction }));
            return updated.IsCompleted;
        }
    }
}
