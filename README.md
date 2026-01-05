# Quasar Mass Cookie Checker V2

A high-performance, multi-threaded Roblox cookie validation tool with comprehensive account analytics.

## Overview

This tool validates Roblox authentication cookies (`_ROBLOSECURITY`) and extracts detailed account information including balances, assets, and security settings. It features parallel processing, proxy support, and organized output formatting.

## Features

- **Mass Validation**: Processes multiple cookies simultaneously using configurable thread count
- **Proxy Support**: Optional proxy integration with automatic rotation
- **Comprehensive Analytics**:
  - Account balance (Robux, pending, credits)
  - RAP (Recent Average Price) valuation
  - Premium status verification
  - Group ownership and funds
  - Security settings (PIN, email verification)
  - Transaction history
  - Account demographics
- **Real-time Progress**: Live statistics display during processing
- **Organized Output**: Separates valid/invalid cookies with detailed account files
- **Error Resilience**: Graceful error handling with detailed logging

## Prerequisites

- **.NET Runtime**: Requires .NET (version compatible with System.Text.Json)
- **Input Files**: 
  - `cookies.txt` - List of Roblox cookies (one per line)
  - `proxies.txt` - Optional proxy list (one per line)

## Installation

1. Ensure .NET runtime is installed on your system
2. Download the compiled executable
3. Place the executable in your desired directory
4. Create required input files in the same directory

## Input File Format

### cookies.txt
```
_cookie_value_here_1
_cookie_value_here_2
_cookie_value_here_3
```

### proxies.txt (Optional)
```
http://proxy1:port
http://proxy2:port
socks5://proxy3:port
```

## Usage

1. **Prepare Input Files**: Create `cookies.txt` with your Roblox cookies
2. **Optional**: Add proxies to `proxies.txt` for rotation
3. **Run Executable**: Launch the application
4. **Monitor Progress**: View real-time statistics and validation results
5. **Review Output**: Check generated files for results

## Output Structure

### Generated Files

- `validcookies.txt` - All valid cookies (raw format)
- `invalidcookies.txt` - All invalid cookies
- `validcookies/` - Directory containing detailed account files:
  - `{username}_cookie.txt` - Individual account reports with full analytics

### Account Report Includes

- Username and User ID
- Robux balance (available, pending, stipends)
- RAP (Recent Average Price) valuation
- Premium membership status
- Group ownership and funds
- Security settings (email verification, PIN status)
- Account creation/birthdate information
- Transaction summaries

## Performance

- **Thread Management**: Uses `Environment.ProcessorCount * 2` threads by default
- **Connection Pooling**: Dedicated HttpClient instances per thread
- **Parallel Processing**: Concurrent cookie validation and data extraction
- **Memory Efficient**: Stream-based file operations for large datasets

## Technical Details

### Validation Process
1. Initial authentication check via Roblox API
2. Concurrent data gathering from multiple endpoints
3. Statistical aggregation in real-time
4. Organized file output with sanitized naming

### API Endpoints Utilized
- Authentication: `users.roproxy.com/v1/users/authenticated`
- Account Info: `accountsettings.roproxy.com/v1/email`
- Security: `accountsettings.roproxy.com/v1/pin`
- Premium: `premiumfeatures.roproxy.com/v1/users/{id}/validate-membership`
- Economy: Multiple endpoints for balances, transactions, and RAP
- Groups: `groups.roproxy.com/v1/users/{id}/groups/roles`
- Inventory: `inventory.roproxy.com/v1/users/{id}/assets/collectibles`

## Security Notes

- **Local Processing**: All operations occur locally; no external data transmission
- **File Security**: Input/output files remain on local filesystem
- **No Persistence**: No cookie data is retained beyond session execution
- **TLS Enforcement**: Enforces TLS 1.2 for all connections

## Error Handling

- **Connection Errors**: Automatic retry with proxy rotation (if configured)
- **API Failures**: Graceful degradation with default values
- **File Operations**: Safe file writing with temporary file merging
- **Thread Safety**: Proper synchronization for shared resources

## Statistics Tracked

- Total RAP (Recent Average Price)
- Total Robux balance
- Total pending Robux
- Total group funds
- Total premium stipends
- Total credits
- Valid/Invalid cookie counts

## Limitations

- Rate limiting may occur without proxies
- Roblox API changes may require updates
- Internet connection required for validation
- Large cookie sets require sufficient system resources

## Support

For issues or questions, ensure:
1. Input files are properly formatted
2. Network connectivity is available
3. Required .NET runtime is installed
4. System has sufficient resources for processing

## Legal Disclaimer

This tool is for educational and authorized testing purposes only. Ensure you have explicit permission to validate any authentication tokens. The developers assume no responsibility for misuse of this software.
