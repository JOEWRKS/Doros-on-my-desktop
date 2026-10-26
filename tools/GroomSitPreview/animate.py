"""Encode generated preview frames, never touch application/artwork inputs."""
import pathlib
import sys
from PIL import Image

root = pathlib.Path(sys.argv[1]).resolve()
frames = [Image.open(p).convert("RGB") for p in sorted(root.glob("[0-9][0-9][0-9].png"))]
assert len(frames) == 144
# APNG retains all PNG colors and the exact 60Hz timing. GIF is a broadly
# viewable fallback with its unavoidable 256-color/10ms timing limitations.
frames[0].save(root.parent / "groom-sit-comparison.png", save_all=True,
               append_images=frames[1:], duration=1000 / 60, loop=0, format="PNG")
palette = frames[40].quantize(colors=256)
indexed = [f.quantize(palette=palette, dither=Image.Dither.NONE) for f in frames]
durations = [20 if i % 3 != 2 else 10 for i in range(144)]
indexed[0].save(root.parent / "groom-sit-comparison.gif", save_all=True,
                append_images=indexed[1:], duration=durations, loop=0, optimize=False)
print(root.parent / "groom-sit-comparison.gif")
