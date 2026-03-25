import json
import base64
import base58
import dag_cbor
from pathlib import Path
from cryptography.hazmat.primitives.asymmetric import ec
from cryptography.hazmat.primitives import hashes
from cryptography.hazmat.primitives.asymmetric.utils import decode_dss_signature, encode_dss_signature

from multiformats import CID, multihash
from fastapi.encoders import jsonable_encoder

class HashtagData:
    issuer = ""
    source = ""
    sourceCid = ""
    files = {}
    tags = {}

class HashtagMeta:
    data = HashtagData()
    sig = ""

def bytes_from_file(file_path: str) -> bytes:
    path = Path(file_path)
    if not path.is_file():
        raise FileNotFoundError(f"File not found: {file_path}")
    return path.read_bytes()

def cid_from_bytes(data: bytes,hash_function_name:str,cid_base_indicator:str,cid_version:int,cid_codec:str) -> list[str]:
    """
    Generate an IPFS CIDv1 (raw, sha2-256) from arbitrary bytes.
    """
    mh = multihash.digest(data, hash_function_name)
    cid = CID(cid_base_indicator, cid_version, cid_codec, mh)
    #return also the multihash as hex
    return [str(mh.hex()),str(cid)]

def signature_from_bytes(data: bytes, privkey_multibase: str) -> str:
    """
    Function generates a signature using Private Key hex 32 based on ED25519
    """
    ecPrivKey_bytes = base58.b58decode(privkey_multibase[1:])
    ecPrivKeyCompressed = ecPrivKey_bytes[2:]
    # private key bytes to int
    ecPrivInt = int.from_bytes(ecPrivKeyCompressed, "big")
    # # Reconstruct P-256 private key
    private_key = ec.derive_private_key(ecPrivInt, ec.SECP256R1())
    # Sign data using key
    der_signature = private_key.sign(data, ec.ECDSA(hashes.SHA256()))
    # convert DER -> r||s raw
    r, s = decode_dss_signature(der_signature)
    raw_sig = r.to_bytes(32, "big") + s.to_bytes(32, "big")
    # base64url encode
    signature_b64 = base64.b64encode(raw_sig).decode()
    return signature_b64

#
# data lineage parameters
hash_function_name = "sha2-256"
cid_base_indicator = "base32"
cid_version = 1
cid_codec = 'raw'

fileBytes= bytes_from_file('hashtag_eo_raster_632e425c_20260219_212629.tif')

hash,cid = cid_from_bytes(fileBytes,hash_function_name,cid_base_indicator,cid_version,cid_codec)
#print(cid)


## create hashtag object
ht = HashtagData()
ht.issuer = "did:web:hashtag.terrasphere.space"
ht.files = {
  "hashtag_eo_raster_632e425c_20260219_212629.tif":  {"cid":cid}
}
ht.source = "https://terrasphere.space"
# calculate cid of 'source' value
sourceHash,sourceCid = cid_from_bytes(ht.source.encode('utf-8'),hash_function_name,cid_base_indicator,cid_version,cid_codec)
ht.sourceCid = sourceCid
tags = {
    "source_order": "JSON with parameters given in original payload",
    "process_graph": "JSON with process graph used at the CDSE background to select and compute EO product from Earth Observation data",
    "eo_product": "GEOTIFF file with EO product suitable for creating task / prescription maps",
    "eo_product_cid": "bafkreihc4hrpcocqqskzb5clg7tnb55c42xpm62sb3dfmqozslvy4wtqam",
    "eo_product_cid_at": "2026-02-19T21:02:01.020972+00:00",
    "proof_type": "JsonWebSignature2020",
    "proof_created": "2026-02-19T21:28:33+00:00Z",
    "proof_verificationMethod": "did:web:hashtag.terrasphere.space#key-1",
    "proof_proofPurpose": "assertionMethod",
    "proof_digest": "bafkreiepzoqd5ymsxymvgcojnakggqjxhmfgdug4rkt5rjxhd3dsg67nfm",
    "proof_signatureValue": "27YjJMcib5JuY0YSmin8G74Umuxo-IcFQsQULbHnNncHLT_nYsmk3SA7UZEsmZNb6hhGCugUQxObcDDiRyouLg",
    "source_order_id": "632e425c-3a0e-4413-ad03-e9dbe93d0517",
    "source_order_cid": "bafkreifkwbqxv4n2rh6gjhqxnr32ybeofy6vnrhz7uwgv7hfx26o3sopvm",
    "source_order_cid_at": "2026-02-19T21:02:01.020969+00:00",
    "process_graph_id": "dd27bcf2-b22a-4ddd-b96b-b9de3771e91c",
    "process_graph_cid": "bafkreihkntf7a447rhlxfwgnrzowkgygtkb72t2tocvai5wc7vsoelxe44",
    "process_graph_cid_at": "2026-02-19T21:02:01.020971+00:00"
}

ht.tags = tags
htmeta = HashtagMeta()
htmeta.data = ht
htmeta.sig = ""

# encode only the 'data' element
htjsonable = jsonable_encoder(htmeta.data)
# encode the json as a dag_cbor object (canonicalized)
dagCborBytes = dag_cbor.encode(htjsonable)

# private key (example, generated earlier)
priKeyMultibase = "z42tiCpEtJnq8G8TkGQU2ibhsah3FwxLQWcAngano8GhEQVm"

#save key to htmeta.sig element
htmeta.sig = signature_from_bytes(dagCborBytes, priKeyMultibase)

# encode the whole htmeta object
htmetajsonable = jsonable_encoder(htmeta)
htmetajson = json.dumps(htmetajsonable, indent=2)
print()
print(htmetajson)
# save to file
with open("hashtag_meta.json", "w") as wf:
  wf.write(htmetajson)



# and verify:
# Load signed metadata.json and proof
with open("hashtag_meta.json", "r") as f:
    metadata = json.load(f)

signature_b64 = metadata["sig"]

# Convert base64url -> raw r||s bytes
raw_sig = base64.b64decode(signature_b64)  # pad if needed
r = int.from_bytes(raw_sig[:32], "big")
s = int.from_bytes(raw_sig[32:], "big")

# public key to check with (example, generated earlier)
pubKeyMultibase = "zDnaeXViy7xZj5bxBjh3LHeGQ84yr2aFaA52ZNcM1QfnfRtZU"

# Decode multibase (remove 'z' prefix)
multicodec_bytes = base58.b58decode(pubKeyMultibase[1:])

# Remove multicodec prefix for P-256 (0x80 0x24)
compressed_pub = multicodec_bytes[2:]

# Reconstruct public key
public_key = ec.EllipticCurvePublicKey.from_encoded_point(ec.SECP256R1(), compressed_pub)

# Recreate DER signature from raw r||s
der_sig = encode_dss_signature(r, s)

# encode the json as a dag_cbor object (canonicalized)
verifyBytes = dag_cbor.encode(metadata["data"])

# Verify the signature
try:
    public_key.verify(
        der_sig,
        verifyBytes,
        ec.ECDSA(hashes.SHA256())
    )
    print("Signature verified successfully!")
except Exception as e:
    print("Signature verification failed:", e)
