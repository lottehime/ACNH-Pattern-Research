#include <stdlib.h>
#include <stdio.h>

long int parse_hex( char* s )
{
	if(!s) return 0;
	if( *s=='0' && *(s+1)=='x' )
	{
		long int r = 0;
		sscanf( s, "%X", &r );
		return r;
	}
	else return(atol( s ));
}

int main( int argc, char* args[] )
{
	const char* USAGE = "Usage: acnhpatternfix [pattern_file]\n"
				        "Set player and town ID bytes in a ACNH design pattern file manually.\n";
	if( ( argc < 2 ) ||
		( strcmp(args[1], "-h") == 0) ||
		( strcmp(args[1], "--help") == 0 )
	  )
	{
		puts(USAGE);
		exit(0);
	}

	if( argc > 2 )
	{
		puts("Incorrect input. Check your invocation.");
		exit(0);
	}

	// This is very verbose and innefficient for real use, just for simple illustration.
	char* filename = NULL;
	char* address[] = { "0x038", "0x039", "0x03A", "0x03B", // Town ID Address (Pattern)
			    "0x054", "0x055", "0x056", "0x057", // Player ID Address (Pattern)
			    "0x03C", "0x03D", "0x03E", "0x03F", "0x040", // ^
			    "0x041", "0x042", "0x043", "0x044", "0x045", // Town Name Address (Pattern) 
			    "0x046", "0x047", "0x048", "0x049", "0x04A", // |
			    "0x04B", "0x04C", "0x04D", "0x04E", "0x04F", // v
			    "0x058", "0x059", "0x05A", "0x05B", "0x05C", // ^
			    "0x05D", "0x05E", "0x05F", "0x060", "0x061", // Player Name Address (Pattern)
			    "0x062", "0x063", "0x064", "0x065", "0x066", // |
			    "0x067", "0x068", "0x069", "0x06A", "0x06B", // v
			    "0x070", "0x071", "0x072", "0x073"		 // Ownership reset flag
			   };

	char* newvalue[] = { "0x016", "0x046", "0x00C", "0x0DD", // Town ID Address (Pattern)
			     "0x07A", "0x08C", "0x06C", "0x0EA", // Player ID Address (Pattern)
			     "0x049", "0x000", "0x073", "0x000", "0x06C", // ^
			     "0x000", "0x061", "0x000", "0x06E", "0x000", // Town Name Address (Pattern) 
			     "0x064", "0x000", "0x04E", "0x000", "0x061", // |
			     "0x000", "0x06D", "0x000", "0x065", "0x000", // v
			     "0x050", "0x000", "0x06C", "0x000", "0x061", // ^
			     "0x000", "0x079", "0x000", "0x065", "0x000", // Player Name Address (Pattern) 
			     "0x072", "0x000", "0x04E", "0x000", "0x061", // |
			     "0x000", "0x06D", "0x000", "0x065", "0x000", // v
			     "0x000", "0x000", "0x000", "0x000"		  // Ownership reset flag
			    };

	if( argc >= 1 ) filename = args[1];
	
	FILE* file = fopen(filename, "rb+");
	if( !file )
	{
		perror(args[0]);
		exit(1);
	}

	for (int i = 0; i < sizeof(address) / sizeof(address[0]); i++) {
		long int offset = parse_hex( address[i] );
		fseek( file, offset, SEEK_SET );
		
		int newvalue_parsed = (int) parse_hex( newvalue[i] );
		if( fputc(newvalue_parsed, file) == EOF )
		{
			perror(args[0]);
			exit(1);
		}
	}

	puts("Done fixing ACNH pattern file.");	
	
	fclose(file);

	exit(0);
	
}
