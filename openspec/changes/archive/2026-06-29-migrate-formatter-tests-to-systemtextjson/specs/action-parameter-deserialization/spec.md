## MODIFIED Requirements

### Requirement: Siren output conversion uses System.Text.Json

The reflection-based `SirenConverter` SHALL produce Siren JSON using System.Text.Json (`JsonObject`/`JsonArray`/`JsonNode`). It MUST NOT depend on Newtonsoft.Json. `IHypermediaJsonConverter.ConvertToJson` SHALL return a `System.Text.Json.Nodes.JsonObject`. The converter MUST preserve the existing Siren wire format, including flat string arrays for `class` and `rel` (e.g. `"rel": ["self"]`).

#### Scenario: Hypermedia object rendered to Siren without Newtonsoft

- **WHEN** an `IHypermediaObject` is converted by `SirenConverter`
- **THEN** the resulting Siren JSON is built with System.Text.Json types
- **AND** no Newtonsoft.Json type participates in the conversion

#### Scenario: rel and class render as flat string arrays

- **WHEN** a hypermedia object with links/classes is converted
- **THEN** `rel` and `class` are emitted as flat arrays of strings (not nested arrays)

#### Scenario: Formatter unit tests assert the output using System.Text.Json

- **WHEN** the `SirenConverter` output is asserted in the formatter unit test suite
- **THEN** the assertions consume the `JsonObject` returned by `ConvertToJson` via System.Text.Json node types
- **AND** no Newtonsoft.Json type is used in those tests
