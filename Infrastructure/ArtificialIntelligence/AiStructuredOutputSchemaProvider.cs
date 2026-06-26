namespace Infrastructure.ArtificialIntelligence;

public sealed class AiStructuredOutputSchemaProvider
{
    public const string CurrentVersion = "ai-trade-validation-schema-v1";

    public string Version => CurrentVersion;

    public string GetSchema()
    {
        return """
            {
              "$schema": "https://json-schema.org/draft/2020-12/schema",
              "$id": "ai-trade-validation-schema-v1",
              "title": "AI trade validation vision extraction",
              "type": "object",
              "additionalProperties": false,
              "required": [
                "triggerId",
                "sceneryId",
                "figureId",
                "frameId",
                "stageId",
                "locationType",
                "confirmationType",
                "isTrendAligned",
                "isPivotZone",
                "visualConfidence"
              ],
              "properties": {
                "triggerId": {
                  "type": ["integer", "null"],
                  "minimum": 1
                },
                "sceneryId": {
                  "type": ["integer", "null"],
                  "minimum": 1
                },
                "figureId": {
                  "type": ["integer", "null"],
                  "minimum": 1
                },
                "frameId": {
                  "type": ["integer", "null"],
                  "minimum": 1
                },
                "stageId": {
                  "type": ["integer", "null"],
                  "minimum": 1
                },
                "locationType": {
                  "type": ["integer", "null"],
                  "enum": [1, 2, 3, null]
                },
                "confirmationType": {
                  "type": ["integer", "null"],
                  "enum": [0, 1, 2, 3, 4, null]
                },
                "isTrendAligned": {
                  "type": ["boolean", "null"]
                },
                "isPivotZone": {
                  "type": ["boolean", "null"]
                },
                "visualConfidence": {
                  "type": ["number", "null"],
                  "minimum": 0,
                  "maximum": 1
                }
              }
            }
            """;
    }
}
