using System;
using System.IO;


namespace SetLAA
{

	/// <summary>Provides methods to get or set the Large Address-Aware flag of an application.</summary>
	internal static class LAA
	{

		private const long NoMZHeader = -1L;
		private const long NoPEHeader = -2L;

		private const short LargeAddressAwareFlag = 0x20;


		private static long GetPEHeaderPosition( Stream input )
		{
			var buffer = new byte[ 4 ];
			
			input.Read( buffer, 0, 2 );
			if( BitConverter.ToInt16( buffer, 0 ) != 0x5A4D )
				return NoMZHeader;

			input.Position = 0x3C;
			input.Read( buffer, 0, 4 );
			var position = BitConverter.ToUInt32( buffer, 0 );

			input.Position = position;
			input.Read( buffer, 0, 4 );
			if( BitConverter.ToInt32( buffer, 0 ) != 0x4550 )
				return NoPEHeader;

			return (long)position;
		}


		/// <summary>Returns a value indicating whether the Large Address-Aware (LAA) flag is set.</summary>
		/// <param name="input">A stream to read the binary image (executable); must not be null. Its position must be at the MZ header.</param>
		/// <returns>Returns a value indicating whether the Large Address-Aware (LAA) flag is set.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="InvalidDataException"/>
		/// <exception cref="NotSupportedException"/>
		public static bool IsLargeAddressAware( Stream input )
		{
			if( input == null )
				throw new ArgumentNullException( "input" );

			if( input.Length == 0L )
				throw new ArgumentException( "Empty stream.", "input" );

			if( !input.CanRead )
				throw new ArgumentException( "Non-readable stream.", "input" );

			if( !input.CanSeek )
				throw new ArgumentException( "Non-seekable stream.", "input" );

			// Get the PE header position:
			var peHeaderPosition = GetPEHeaderPosition( input );
			if( peHeaderPosition < 0L )
			{
				if( peHeaderPosition == NoMZHeader )
					throw new InvalidDataException( "MZ header not found." );
				throw new NotSupportedException( "PE header not found." );
			}

			try
			{
				input.Position = peHeaderPosition + 0x16;
				var buffer = new byte[ 2 ];
				input.Read( buffer, 0, 2 );
				return ( BitConverter.ToInt16( buffer, 0 ) & LargeAddressAwareFlag ) == LargeAddressAwareFlag;
			}
			catch( Exception )
			{
				throw new InvalidDataException( "Invalid field position." );
			}
		}


		/// <summary>Tries to set or clear the Large Address-Aware (LAA) flag, if not already set or cleared.</summary>
		/// <param name="stream">A stream to read the executable. Its position must be at the MZ header.</param>
		/// <param name="enable">True to set the Large Address-Aware (LAA) flag, false to clear it.</param>
		/// <returns>Returns true if the flag has been modified, otherwise returns false.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="InvalidDataException"/>
		/// <exception cref="NotSupportedException"/>
		public static bool SetLargeAddressAware( Stream stream, bool enable )
		{
			if( stream == null )
				throw new ArgumentNullException( "stream" );

			if( stream.Length == 0L )
				throw new ArgumentException( "Empty stream.", "stream" );

			if( !stream.CanRead )
				throw new ArgumentException( "Non-readable stream.", "stream" );

			if( !stream.CanWrite )
				throw new ArgumentException( "Read-only stream.", "stream" );

			if( !stream.CanSeek )
				throw new ArgumentException( "Non-seekable stream.", "stream" );

			// Get the PE header position:
			var peHeaderPosition = GetPEHeaderPosition( stream );
			if( peHeaderPosition < 0L )
			{
				if( peHeaderPosition == NoMZHeader )
					throw new InvalidDataException( "MZ header not found." );
				throw new NotSupportedException( "PE header not found." );
			}

			var flagsPosition = peHeaderPosition + 0x16;

			try
			{
				stream.Position = flagsPosition;
			}
			catch( Exception )
			{
				throw new InvalidDataException( "Invalid field position." );
			}

			var buffer = new byte[ 2 ];
			stream.Read( buffer, 0, 2 );
			var flags = BitConverter.ToInt16( buffer, 0 );

			if( ( ( flags & LargeAddressAwareFlag ) == LargeAddressAwareFlag ) == enable )
				return false;
			// already set or clear

			if( enable )
				flags |= LargeAddressAwareFlag;
			else
				flags &= ~LargeAddressAwareFlag;

			buffer = BitConverter.GetBytes( flags );

			// update:
			stream.Position = flagsPosition;
			stream.Write( buffer, 0, 2 );

			return true;
		}


		/// <summary>Sets the state of the Large Address-Aware (LAA) flag of an application, given its file name.</summary>
		/// <param name="applicationFileName">The full path to the application file name.</param>
		/// <param name="state">True to set the LAA flag, false to clear it.</param>
		/// <returns>Returns true if the LAA flag has been modified, otherwise returns false.</returns>
		/// <exception cref="ArgumentNullException"/>
		/// <exception cref="ArgumentException"/>
		/// <exception cref="InvalidOperationException"/>
		public static bool SetLargeAddressAware( string applicationFileName, bool state )
		{
			if( applicationFileName == null )
				throw new ArgumentNullException( "applicationFileName" );

			try
			{
				applicationFileName = Path.ChangeExtension( applicationFileName, "exe" );
			}
			catch( ArgumentException )
			{
				throw new ArgumentException( "Invalid file name.", "applicationFileName" );
			}


			var tempFileName = Path.ChangeExtension( applicationFileName, "laa" );
			try
			{
				if( File.Exists( tempFileName ) )
				{
					File.SetAttributes( tempFileName, File.GetAttributes( tempFileName ) & ~FileAttributes.ReadOnly );
					File.Delete( tempFileName );
				}
				File.Copy( applicationFileName, tempFileName );
			}
			catch( Exception ex )
			{
				throw new InvalidOperationException( "Failed to initialize temp file.", ex );
			}


			var result = false;
			try
			{
				using( var stream = new FileStream( tempFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None ) )
					if( IsLargeAddressAware( stream ) != state )
						result = SetLargeAddressAware( stream, state );
			}
			catch( InvalidDataException )
			{
				throw;
			}
			catch( NotSupportedException )
			{
				throw;
			}


			if( result )
			{
				File.SetAttributes( applicationFileName, File.GetAttributes( applicationFileName ) & ~FileAttributes.ReadOnly );
				File.Delete( applicationFileName );
				File.Copy( tempFileName, applicationFileName );
			}
			
			File.Delete( tempFileName );
			
			return result;
		}

	}

}