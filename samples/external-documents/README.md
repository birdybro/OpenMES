# Sample external documents

These are empty placeholder files used by the `FileSystemDocumentConnector`
reference implementation. Filenames follow the convention documented in that
class:

| Pattern                             | DocumentType    | Scope                |
|-------------------------------------|-----------------|----------------------|
| `{part}_{rev}.{ext}`                | Drawing         | Part + Revision      |
| `{part}_{rev}_wi.{ext}`             | WorkInstruction | Part + Revision      |
| `{part}_{rev}_procedure.{ext}`      | Procedure       | Part + Revision      |
| `{part}_{rev}_quality.{ext}`        | QualityDocument | Part + Revision      |
| `{part}_{rev}_op_{opcode}_{type}.{ext}` | varies      | Part + Revision + Op |
| `resource_{code}_{type}.{ext}`      | varies          | Resource             |

`random-unrecognised-file.txt` exists to verify that the connector silently
skips files that don't match any pattern.

Real PDFs are not checked in — the operator portal will happily list these
references and trust the filesystem to actually contain the documents.
