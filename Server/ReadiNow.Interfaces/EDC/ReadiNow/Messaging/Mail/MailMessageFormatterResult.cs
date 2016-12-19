// Copyright 2011-2016 Global Software Innovation Pty Ltd
namespace EDC.SoftwarePlatform.Interfaces.EDC.ReadiNow.Messaging.Mail
{
	/// <summary>
	///     Mail Message formatter result.
	/// </summary>
	public enum MailMessageFormatterResult
	{
		/// <summary>
		///     The formatter ran without problem.
		/// </summary>
		Ok = 0,

		/// <summary>
		///     The formatter was skipped.
		/// </summary>
		Skip = 1,

		/// <summary>
		///     The formatter determined that the message should be rejected
		/// </summary>
		Reject = 2,

		/// <summary>
		///     An error occurred processing the formatter.
		/// </summary>
		Error = 3
	}
}