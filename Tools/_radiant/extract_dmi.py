"""Extract selected DMI states into RSI, preserving directions and frame delays."""
import argparse
import json
import re
from pathlib import Path
from PIL import Image


def states(path):
    image = Image.open(path).convert("RGBA")
    description = Image.open(path).info["Description"]
    width = int(re.search(r"width = (\d+)", description)[1])
    height = int(re.search(r"height = (\d+)", description)[1])
    offset = 0
    result = {}
    for name, data in re.findall(r'state = "([^"]+)"(.*?)(?=state = |# END DMI|\Z)', description, re.S):
        dirs = int(re.search(r"dirs = (\d+)", data)[1])
        frames = int(re.search(r"frames = (\d+)", data)[1])
        delay = re.search(r"delay = ([\d.,]+)", data)
        delays = [float(x) / 10 for x in delay[1].split(",")] if delay else [0.1] * frames
        result[name] = (offset, dirs, frames, delays)
        offset += dirs * frames
    return image, width, height, result


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("source", type=Path)
    parser.add_argument("output", type=Path)
    parser.add_argument("mapping", nargs="+", help="source-state=output-state")
    args = parser.parse_args()
    sheet, width, height, available = states(args.source)
    args.output.mkdir(parents=True, exist_ok=True)
    metadata = {"version": 1, "license": "CC-BY-SA-3.0", "copyright": "Extracted from upstream game assets; see source.txt", "size": {"x": width, "y": height}, "states": []}
    for mapping in args.mapping:
        source, target = mapping.split("=")
        start, dirs, frames, delays = available[source]
        # DMI is frame-major; RSI packs each direction's frames consecutively.
        output = Image.new("RGBA", (width * frames, height * dirs))
        for direction in range(dirs):
            for frame in range(frames):
                cell = start + frame * dirs + direction
                x, y = cell % (sheet.width // width) * width, cell // (sheet.width // width) * height
                output.paste(sheet.crop((x, y, x + width, y + height)), (frame * width, direction * height))
        output.save(args.output / (target + ".png"))
        entry = {"name": target, "directions": dirs}
        if frames > 1:
            entry["delays"] = [delays] * dirs
        metadata["states"].append(entry)
    (args.output / "meta.json").write_text(json.dumps(metadata, indent=2) + "\n", encoding="utf-8")
