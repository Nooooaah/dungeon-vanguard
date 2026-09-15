import imageio_ffmpeg
import subprocess

ffmpeg = imageio_ffmpeg.get_ffmpeg_exe()
input_path = r'E:\git_home\dungeon_-vanguard\My project\Assets\StreamingAssets\intro.mp4'
output_path = r'E:\git_home\dungeon_-vanguard\My project\Assets\StreamingAssets\intro_fixed.mp4'

# Re-encode to standard H.264 with proper color space
cmd = [
    ffmpeg, '-y',
    '-i', input_path,
    '-c:v', 'libx264',
    '-pix_fmt', 'yuv420p',
    '-colorspace', 'bt709',
    '-color_primaries', 'bt709',
    '-color_trc', 'bt709',
    '-preset', 'fast',
    '-crf', '23',
    '-c:a', 'aac',
    '-b:a', '128k',
    output_path
]

print("Running ffmpeg...")
result = subprocess.run(cmd, capture_output=True, text=True)
print("Return code:", result.returncode)
if result.stderr:
    # Print last few lines of stderr (ffmpeg outputs progress to stderr)
    lines = result.stderr.strip().split('\n')
    for line in lines[-5:]:
        print(line)

import os
if os.path.exists(output_path):
    size = os.path.getsize(output_path)
    print(f"Output file size: {size} bytes")
    
    # Replace original
    import shutil
    shutil.copy(output_path, input_path)
    print("Replaced original intro.mp4 with fixed version")
else:
    print("Output file not created!")
