using System;
using System.Globalization;
using System.IO;


namespace SetLAA
{
	using Properties;


	internal static class Program
	{
		
		/// <summary>The main application entry point.</summary>
		/// <param name="args">Arguments from the command-line.</param>
		internal static void Main( string[] args )
		{
			if( args.Length == 0 )
			{
				Console.WriteLine( Resources.MissingTargetAppFileName );
				Console.WriteLine( Resources.Syntax );
				return;
			}

			var appFileName = Path.ChangeExtension( args[ 0 ], "exe" );
			if( !File.Exists( appFileName ) )
			{
				Console.WriteLine( string.Format( CultureInfo.InvariantCulture, Resources.FileNotFound, appFileName ) );
				return;
			}

			var state = true;
			if( args.Length > 1 )
				state = ( args[ 1 ] != "0" );

			// TODO - make a backup copy of the target application !
			var backupFileName = Path.ChangeExtension( appFileName, "original" );
			if( File.Exists( backupFileName ) )
				File.Delete( backupFileName );
			File.Copy( appFileName, backupFileName );

			try
			{
				if( !LAA.SetLargeAddressAware( appFileName, state ) )
					Console.WriteLine( state ? Resources.AlreadySet : Resources.AlreadyCleared );
			}
			catch( Exception ex )
			{
				Console.WriteLine( ex.Message );
			}
		}

	}

}