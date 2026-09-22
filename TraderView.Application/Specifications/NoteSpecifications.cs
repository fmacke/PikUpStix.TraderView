using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TraderView.Application.Interfaces.Persistence;
using TraderView.Domain.Entities;

namespace TraderView.Application.Specifications
{
    public class NoteByPositionSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        public NoteByPositionSpecification(int positionId)
        {
            Criteria = n => n.PositionId == positionId;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }

    public class NoteByTradeExecutionSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        public NoteByTradeExecutionSpecification(int tradeExecutionId)
        {
            Criteria = n => n.TradeExecutionId == tradeExecutionId;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }

    public class NoteByTradeTypeSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        public NoteByTradeTypeSpecification(int tradeTypeId)
        {
            Criteria = n => n.TradeTypeId == tradeTypeId;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }

    public class NoteByDateRangeSpecification : ISpecification<Note>
    {
        public Expression<Func<Note, bool>>? Criteria { get; }
        public List<Expression<Func<Note, object>>> Includes { get; } = new();
        public List<string> IncludeStrings { get; } = new();
        public List<(Expression<Func<Note, object>> KeySelector, bool IsDescending)> OrderBys { get; } = new();
        public int? Take { get; } = null;
        public int? Skip { get; } = null;
        public bool IsPagingEnabled { get; } = false;

        public NoteByDateRangeSpecification(DateTime startDate, DateTime endDate)
        {
            Criteria = n => n.EntryDate >= startDate && n.EntryDate <= endDate;
            OrderBys.Add((n => n.EntryDate, true));
        }
    }
}
