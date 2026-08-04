using MetaCompta.Application.DTOs;

namespace MetaCompta.BackOffice.Services;

/// <summary>
/// Generic typed HTTP client for communicating with the MetaCompta API.
/// Handles JWT token injection, serialization/deserialization, and error handling.
/// </summary>
public interface IApiClient
{
    Task<ApiResponse<T>?> GetAsync<T>(string url, CancellationToken ct = default);
    Task<ApiResponse<T>?> PostAsync<T>(string url, object data, CancellationToken ct = default);
    Task<ApiResponse<T>?> PutAsync<T>(string url, object data, CancellationToken ct = default);
    Task<ApiResponse<T>?> DeleteAsync<T>(string url, CancellationToken ct = default);
    Task<byte[]?> GetBytesAsync(string url, CancellationToken ct = default);
}
