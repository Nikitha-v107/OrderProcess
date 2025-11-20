using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Threading.Tasks;

namespace ProductCatalogue.Function
{
    public class OrderProcessorService
    {
        private readonly string _connectionString;
        private readonly ILogger<OrderProcessorService> _logger;

        public OrderProcessorService(IConfiguration configuration, ILogger<OrderProcessorService> logger)
        {
            // Read from ConnectionStrings:SqlConnection in local.settings.json
            // {
            //   "ConnectionStrings": {
            //     "SqlConnection": "..."
            //   }
            // }
            _connectionString = configuration.GetConnectionString("SqlConnection")
                               ?? configuration["ConnectionStrings:SqlConnection"]
                               ?? string.Empty;

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("SQL connection string is not configured. Please set 'ConnectionStrings:SqlConnection' in local.settings.json.");
            }

            _logger = logger;
        }

        public async Task UpdateOrderAndStock(int orderId, int productId, int quantity)
        {
            _logger.LogInformation($"Starting order processing for OrderId={orderId}, ProductId={productId}, Qty={quantity}");

            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // STEP 1 — Update Order Status -> Processing
                const string updateOrderProcessingSql =
                    "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";

                await using (var cmd = new SqlCommand(updateOrderProcessingSql, connection, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@Status", "Processing");
                    cmd.Parameters.AddWithValue("@OrderId", orderId);

                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"Order {orderId} updated to 'Processing'.");
                }

                // STEP 2 — Reduce stock
                const string updateStockSql =
                    "UPDATE Products SET Stock = Stock - @Quantity WHERE Id = @ProductId";

                await using (var cmd = new SqlCommand(updateStockSql, connection, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@Quantity", quantity);
                    cmd.Parameters.AddWithValue("@ProductId", productId);

                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"Product {productId} stock decreased by {quantity}.");
                }

                // STEP 3 — Update Order Status -> Completed
                const string updateOrderCompletedSql =
                    "UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";

                await using (var cmd = new SqlCommand(updateOrderCompletedSql, connection, (SqlTransaction)transaction))
                {
                    cmd.Parameters.AddWithValue("@Status", "Completed");
                    cmd.Parameters.AddWithValue("@OrderId", orderId);

                    await cmd.ExecuteNonQueryAsync();
                    _logger.LogInformation($"Order {orderId} updated to 'Completed'.");
                }

                // COMMIT TRANSACTION
                await transaction.CommitAsync();
                _logger.LogInformation($"OrderId {orderId} processed successfully. Transaction committed.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error processing order {orderId}. Transaction rollback executed.");
                throw; // Let the Function handle it
            }
        }
    }
}
