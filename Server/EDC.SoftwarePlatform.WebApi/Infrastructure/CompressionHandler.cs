// Copyright 2011-2016 Global Software Innovation Pty Ltd

using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EDC.SoftwarePlatform.WebApi.Infrastructure
{
	/// <summary>
	///     Compression handler.
	/// </summary>
	public class CompressionHandler : DelegatingHandler
	{
		/// <summary>
		///     Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send to the server.</param>
		/// <param name="cancellationToken">A cancellation token to cancel operation.</param>
		/// <returns>
		///     Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
		/// </returns>
		protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
		{
			return base.SendAsync( request, cancellationToken ).ContinueWith( responseToCompleteTask =>
			{
				HttpResponseMessage response = responseToCompleteTask.Result;

				if ( response.RequestMessage.Headers.AcceptEncoding != null &&
				     response.RequestMessage.Headers.AcceptEncoding.Count > 0 )
				{
					string encodingType = response.RequestMessage.Headers.AcceptEncoding.First( ).Value;                    

                    if (encodingType != "identity")
				    {
                        response.Content = new CompressedContent(response.Content, encodingType);				        
				    }					
				}

				return response;
			}, TaskContinuationOptions.OnlyOnRanToCompletion );
		}
	}
}