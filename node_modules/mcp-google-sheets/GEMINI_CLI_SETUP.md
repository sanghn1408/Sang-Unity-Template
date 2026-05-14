# Gemini CLI Setup Guide

This guide shows how to configure the Google Sheets MCP server with Gemini CLI using OAuth tokens from your database.

## Overview

The server now supports **direct OAuth token injection** via environment variables, eliminating the need for token files. This is perfect for Gemini CLI's OAuth flow where tokens are stored in a database.

## Configuration

### 1. Gemini CLI settings.json

Location: `~/.config/gemini-cli/settings.json`

```json
{
  "mcpServers": {
    "google-sheets": {
      "command": "node",
      "args": ["/home/filipp/blackbox/mcp-google-sheets/dist/index.js"],
      "env": {
        "GOOGLE_SHEETS_CLIENT_ID": "${GOOGLE_SHEETS_CLIENT_ID}",
        "GOOGLE_SHEETS_CLIENT_SECRET": "${GOOGLE_SHEETS_CLIENT_SECRET}",
        "GOOGLE_SHEETS_ACCESS_TOKEN": "${GOOGLE_SHEETS_ACCESS_TOKEN}",
        "GOOGLE_SHEETS_REFRESH_TOKEN": "${GOOGLE_SHEETS_REFRESH_TOKEN}",
        "GOOGLE_SHEETS_TOKEN_EXPIRY": "${GOOGLE_SHEETS_TOKEN_EXPIRY}"
      }
    }
  }
}
```

### 2. Environment Variables from Database

When Gemini CLI retrieves OAuth tokens from your database, it should set these environment variables:

```bash
# Required
export GOOGLE_SHEETS_CLIENT_ID="your-client-id.apps.googleusercontent.com"
export GOOGLE_SHEETS_CLIENT_SECRET="your-client-secret"
export GOOGLE_SHEETS_ACCESS_TOKEN="ya29.a0AfB_byD..."

# Optional but recommended
export GOOGLE_SHEETS_REFRESH_TOKEN="1//0gH..."
export GOOGLE_SHEETS_TOKEN_EXPIRY="2025-11-25T10:34:45.248Z"
```

### 3. Mapping from Your Database Format

Based on your database structure:

```javascript
// Your DB record
{
  "accessToken": "28336761071998be58e52f2b7c0fb4f6:...",
  "expiresAt": "2025-11-25T10:34:45.248Z",
  "tokenType": "Bearer",
  "scopes": ["https://www.googleapis.com/auth/spreadsheets", ...],
  // refreshToken might be stored separately
}

// Map to environment variables
GOOGLE_SHEETS_ACCESS_TOKEN = record.accessToken
GOOGLE_SHEETS_TOKEN_EXPIRY = record.expiresAt
GOOGLE_SHEETS_REFRESH_TOKEN = record.refreshToken  // if available
```

## Authentication Priority

The server checks authentication methods in this order:

1. **CREDENTIALS_CONFIG** (base64 service account)
2. **SERVICE_ACCOUNT_PATH** (service account file)
3. **Direct OAuth Token** (GOOGLE_SHEETS_ACCESS_TOKEN) ← **NEW!**
4. OAuth with token file (TOKEN_PATH)
5. OAuth with credentials file (CREDENTIALS_PATH)
6. Application Default Credentials (ADC)

## Token Refresh

If you provide a `GOOGLE_SHEETS_REFRESH_TOKEN` and the access token is expired, the server will:

1. Automatically refresh the token
2. Log the new token information to stderr
3. Continue operating with the new token

**Important:** You should capture the new token from the logs and update your database.

### Example Refresh Log Output

```json
{
  "accessToken": "ya29.a0AfB_byD...",
  "expiresAt": "2025-11-25T11:34:45.248Z",
  "refreshToken": "1//0gH..."
}
```

## Complete Example

### Step 1: Build the Server

```bash
cd /home/filipp/blackbox/mcp-google-sheets
npm install
npm run build
```

### Step 2: Configure Gemini CLI

Edit `~/.config/gemini-cli/settings.json`:

```json
{
  "mcpServers": {
    "google-sheets": {
      "command": "node",
      "args": ["/home/filipp/blackbox/mcp-google-sheets/dist/index.js"],
      "env": {
        "GOOGLE_SHEETS_CLIENT_ID": "123456789.apps.googleusercontent.com",
        "GOOGLE_SHEETS_CLIENT_SECRET": "GOCSPX-abc123...",
        "GOOGLE_SHEETS_ACCESS_TOKEN": "ya29.a0AfB_byD...",
        "GOOGLE_SHEETS_REFRESH_TOKEN": "1//0gH...",
        "GOOGLE_SHEETS_TOKEN_EXPIRY": "2025-11-25T10:34:45.248Z"
      }
    }
  }
}
```

### Step 3: Test the Server

```bash
# Set environment variables
export GOOGLE_SHEETS_CLIENT_ID="123456789.apps.googleusercontent.com"
export GOOGLE_SHEETS_CLIENT_SECRET="GOCSPX-abc123..."
export GOOGLE_SHEETS_ACCESS_TOKEN="ya29.a0AfB_byD..."
export GOOGLE_SHEETS_REFRESH_TOKEN="1//0gH..."
export GOOGLE_SHEETS_TOKEN_EXPIRY="2025-11-25T10:34:45.248Z"

# Run the server
node dist/index.js
```

You should see:
```
Starting Google Sheets MCP Server...
Trying OAuth2 with direct token from environment variables
Successfully authenticated using OAuth2 with direct token
Google Sheets services initialized successfully
Google Sheets MCP Server running on stdio
```

## Integration with Gemini CLI OAuth Flow

When Gemini CLI handles the OAuth flow:

1. User authenticates via browser
2. Gemini CLI receives OAuth response
3. Gemini CLI stores tokens in database
4. When launching MCP server, Gemini CLI:
   - Retrieves tokens from database
   - Sets environment variables
   - Launches the server with those variables

## Advantages of This Approach

✅ **No token files** - Everything via environment variables  
✅ **Database-driven** - Tokens stored centrally  
✅ **Automatic refresh** - Server handles token refresh  
✅ **Secure** - Tokens never written to disk  
✅ **Multi-user** - Each user can have their own tokens  

## Environment Variables Reference

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `GOOGLE_SHEETS_CLIENT_ID` | Yes | OAuth Client ID | `123.apps.googleusercontent.com` |
| `GOOGLE_SHEETS_CLIENT_SECRET` | Yes | OAuth Client Secret | `GOCSPX-abc123...` |
| `GOOGLE_SHEETS_ACCESS_TOKEN` | Yes | Current access token | `ya29.a0AfB_byD...` |
| `GOOGLE_SHEETS_REFRESH_TOKEN` | Recommended | Refresh token for auto-refresh | `1//0gH...` |
| `GOOGLE_SHEETS_TOKEN_EXPIRY` | Recommended | Token expiry (ISO 8601) | `2025-11-25T10:34:45.248Z` |
| `DRIVE_FOLDER_ID` | Optional | Default Drive folder | `1aBcDeFgHiJkLmN` |

## Troubleshooting

### "OAuth2 token not found"

**Cause:** Missing `GOOGLE_SHEETS_ACCESS_TOKEN` environment variable

**Solution:** Ensure Gemini CLI is setting the access token from the database

### "Token expired" with no refresh

**Cause:** Missing `GOOGLE_SHEETS_REFRESH_TOKEN`

**Solution:** Store and provide the refresh token from the OAuth flow

### "Invalid credentials"

**Cause:** Token is invalid or revoked

**Solution:** Re-authenticate through Gemini CLI's OAuth flow

## Security Notes

1. **Never log tokens** in production
2. **Encrypt tokens** in your database
3. **Use HTTPS** for all OAuth flows
4. **Rotate tokens** regularly
5. **Limit scopes** to minimum required

## Support

For issues or questions:
- GitHub Issues: https://github.com/xing5/mcp-google-sheets/issues
- Documentation: https://github.com/xing5/mcp-google-sheets
