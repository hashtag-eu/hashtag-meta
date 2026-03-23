# Hashtag Meta Design

This document describes the design of the proposed Hashtag metadata functionality and the JSON file format for the `hashtag_meta.json` file.

## Concepts

The design concept for the hashtag metadata functionality have been formalized after the MVP test meeting on February 9th 2026.

## Json file format

The `hashtag_meta.json` file format contains a `data` and `sig` root element.

The `data` element contains the following fields:

- `issuer`: string value containing the issuer's DID (required)
- `files`: map of file names with their calculated CID as a json map with the key `cid` and the CID as string (required)
- `source`: link to the source (URI) (optional)
- `sourceCid`: string value with the CID for the value in `source` (required when `source` element has a value)
- `tags`: map of key-value pairs to store additional information by the issuer (optional)

The `sig` is the signature field, which contains the signed hash of the `data` element, to allow verification of the `data` element's content
and show that it has not been altered. The `data` element's json is first encoded as a [Dag-Cbor](https://ipld.io/specs/codecs/dag-cbor/spec/)
object to create canonicalized json (removing whitespace, ordering dictionary elements, etc), so the layout/ordering of the json of the `data` element
doesn't alter the calculated hash of the element's data content.

## Hashtag Metadata file example

```json
{
  "data": {
    "issuer": "did:web:farmmaps.eu", //required: string DID format
    "tags": { //optional: dictionary of key-value pairs as strings
      "key1": "value1",
      "key2": "value2"
    },
    "source": "https://www.farmmaps.eu", //optional: source string (URI format)
    "sourceCid": "bafkreibo4xlwpfoticzmpcjqwck3kfppfllzqqp24xizlgys2ahjprzygi",  //optional: calculated CID for the value of 'source' above
    "files": { //required: dictionary of file names containing an object with the file CID calculated from the (binary) file contents
      "file1.txt": { "cid": "bafkreid422tw22w3jpfey6r2brvfe6xzjhm3wrixprhsj7mrinaxrcoohq" },
      "icon.png" : { "cid": "bafkreib45zige2aedcl73valp6b574biv76weojeanzftvpi7g3pffxbre" }
    }
  },
   //required: signed base64 encoded hash of the 'data' block above, calculation steps described below
  "sig": "JIbhbE6l4hl7kGjLa0QM2iPp1FHTSBESllgmPR7NxeVvDetgXaakY2NKCxxtNv7qO+SURR9G+7AodVuXXkE+OQ=="
}
```

## Signature of the hashtag metadata file

The `hashtag_meta.json` contains a signature as a root element that is calculated as follows:

- The `data` element's value is read into a DAG-CBOR object from which a canonicalized json byte array is derived.
- The SHA256 hash is calculated for the canonicalized json byte array.
- This hash is signed with the issuer's private key and the signed bytes are saved as Base64 string in the `sig` element.
- The issuer's public key is made available in the public well known DID document \
  for example
- The `hashtag_meta.json` is saved with the signature written in the `sig` element.

## Verification

For verification of the `hashtag_meta.json` file and included files, the public key of the issuer is needed.

This should be published as a `verificationMethod` in a DID document definition, where the key `#hashtag_meta` is appended to the issuer,
to identify the verification method for Hashtag, for example:

```json
//https://pods.farmpod.eu/.well-known/did.json
{
  "@context": [
    "https://www.w3.org/ns/did/v1",
    "https://w3id.org/security/suites/jws-2020/v1"
  ],
  "id": "did:web:pods.farmpod.eu",
  "alsoKnownAs": [],
  "authentication": [],
  "verificationMethod": [
    {
      "id": "did:web:pods.farmpod.eu#hashtag_meta",
      "type": "Multikey",
      "controller": "did:web:pods.farmpod.eu",
      "publicKeyMultibase": "zDnaezKwM4pVSdot6VBezsJmw7sGARi4o9jERcHsPVrX8wqaA"
    }
  ],
  "service": []
}
```

## Relevant links

Relevant links to the information of items described in this document.

- For more information about cid's (content identifier) see <https://dasl.ing/>  (specification  and links to libraries implementing it )
- Spec did:web <https://w3c-ccg.github.io/did-method-web>
- Spec did:key <https://w3c-ccg.github.io/did-key-spec/> ( used for the farmer )
- Cid inspector <https://cid.ipfs.tech/>
- Example of a did web ( did:web:pods.farmpod.eu  -> <https://pods.farmpod.eu/.well-known/did.json> )
- Universal resolver ( test dids ) <https://dev.uniresolver.io/>
- Dag-Cbor <https://ipld.io/specs/codecs/dag-cbor/spec/>
