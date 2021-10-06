using System;
using System.Collections.Generic;
using System.Text;
using OrderCloud.SDK;


namespace ordercloud.integrations.vertex
{
	public interface IVertexCommand
	{
		// Use this before checkout. No records will be saved in avalara.
		Task<TransactionModel> GetEstimateAsync(OrderWorksheet orderWorksheet);
		// Use this during submit.
		Task<TransactionModel> CreateTransactionAsync(OrderWorksheet orderWorksheet);
		// Committing the transaction makes it eligible to be filed as part of a tax return. 
		// When should we do this? 
		Task<TransactionModel> CommitTransactionAsync(string transactionCode);
	}
}
