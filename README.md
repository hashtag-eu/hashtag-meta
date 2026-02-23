# Hashtag Meta

This solution contains the Hashtag Meta command line tool to create, sign and validate a Hashtag metadata json file.

The tool can also create a multibase public / private keypair to sign / verify the signature of the hashtag metadata file.

## Usage

`./htmeta.exe <verb> [options]`

### Commandline verbs
|Command|Description|
|-|-|
|create, c|Create a new Hashtag metadata file from input files or folder|
|createkeypair, k, kp, createkey|Create a new Hashtag public and private key, optionally in an output file|
|sign, s|Sign a Hashtag metadata file using public and private key pair|
|verify, v|Verify a Hashtag metadata file using public key|
|help|Display more information on a specific command|
|version|Display version information.|

### Commandline options

The following options are available for all commands:

|Option|Description|
|-|-|
|-w, --wait|Keep console open after command completes (press key to exit)|
|-v, --verbose|Verbose output to console|

### `Create` options

Syntax: `./htmeta.exe c(reate) [filename] -i <files|folder|zip> [-t|-f] [-d <metatemplate>] [-p <private-key>] [-k <public-key>] [-z <output.zip>]`

|Option|Description|
|-|-|
|[filename]|specify file name of metadata file (default `hashtag_meta.json`)|
|-i, --input (required)|List of file names, folder name or zip file name|
|-t, --type|Type of input `Auto|Files|Folder|Zip`, default `Auto`|
|-f, --force|Force create the hashtag_meta json file or output zip file, overwrite existing file|
|-d, --data|Hashtag Meta Data Json string or file name to initialize the hashtag metadata|
|-p, --private-key|Private key to use to sign (multibase syntax)|
|-k, --public-key|Public key to use to sign (multibase syntax)|
|-z, --create-zip|Create signed output zip file of the contents including the metadata file|


### `Create key pair` options

Create a new key pair, show output in console, optionally save to file.

Syntax: `./htmeta.exe createkeypair [-o keypairfile.txt]`

|Option|Description|
|-|-|
|-o, --output|Output text file to save the private/public key pair to (optionsal)|

### `Sign` options

Sign a hashtag metadata file using the private and public key pair.
If the hashtag metadata Data block contains a `source` entry, a `sourceCID` will be calculated and saved to the block before the hash is calculated.

Syntax: `./htmeta.exe s(ign) [filename] -p <private-key> -k <public-key>`

|Option|Description|
|-|-|
|-p, --private-key|Private key to use (multibase syntax)|
|-k, --public-key|Public key to use (multibase syntax)|

### `Verify` options

Verify a hashtag metadata file using the public key. Checks that the Data block's signature matches the provided public key, checks the file CIDs of the files listed in the Data block, and optionally the sourceCID with the `source` element.

Syntax: `./htmeta.exe verify [hashtag_meta.json] -k <public-key>`

|Option|Description|
|-|-|
|[filename]|specify file name of metadata file (default `hashtag_meta.json`)|
|-k, --public-key|Public key in multibase syntax, to check the input hashtag metadata file with|
|-t, --type|Type of input `Auto|Files|Folder|Zip`, default `Auto`|
|-i, --input (required)|List of file names, folder name or zip file name|
