import imageio_ffmpeg
import subprocess
import shutil

ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
input_path = r'E:\git_home\dungeon_-vanguard\My project\Assets\StreamingAssets\intro.mp4'
output_path = r'E:\git_home\dungeon_-vanguard\My project\Assets\StreamingAssets\intro_fixed2.mp4'

# Encode with baseline profile for maximum compatibility
cmd = [
    ffmpeg, '-y',
    '-i', input_path,
    '-c:v', 'libx264',
    '-profile:v', 'baseline',
    '-level', '3.1',
    '-pix_fmt', 'yuv420p',
    '-colorspace', 'bt709',
    '-color_primaries', 'bt709',
    '-color_trc', 'bt709',
    '-preset', 'slow',
    '-crf', '20',
    '-an',  # remove audio to simplify
    output_path
]

print("Encoding with baseline profile...")
result = subprocess.run(cmd, capture_output=True, text=True)
print("Return code:", result.returncode)
lines = result.stderr.strip().split('\n')
for line in lines[-3:]:
    print(line)

import os
if os.path.exists(output_path):
    print(f"Output size: {os.path.getsize(output_path)} bytes")
    shutil.copy(output_path, input_path)
    print("Replaced intro.mp4")
else:
    print("FAILED")
