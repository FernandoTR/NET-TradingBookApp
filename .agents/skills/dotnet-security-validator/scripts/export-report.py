import json
from datetime import datetime
from pathlib import Path

report = {
    "generated_at": datetime.utcnow().isoformat(),
    "status": "completed",
    "findings": []
}

output = Path("security-report.json")
output.write_text(json.dumps(report, indent=2))

print(f"Report exported to {output}")
