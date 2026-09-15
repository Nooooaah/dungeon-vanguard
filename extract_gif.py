from PIL import Image
import os

gif_path = r'E:\git_home\dungeon_-vanguard\My project\Assets\Art\UI\background\18683830.gif'
out_dir = r'E:\git_home\dungeon_-vanguard\My project\Assets\Art\UI\background\gif_frames'

os.makedirs(out_dir, exist_ok=True)

img = Image.open(gif_path)
print(f'Frames: {img.n_frames}')
print(f'Size: {img.size}')

durations = []
for i in range(img.n_frames):
    img.seek(i)
    duration = img.info.get('duration', 100)
    durations.append(duration)
    frame = img.convert('RGBA')
    frame.save(os.path.join(out_dir, f'frame_{i:03d}.png'))
    print(f'Frame {i}: {duration}ms saved')

# Write durations to a file for Unity to read
with open(os.path.join(out_dir, 'durations.txt'), 'w') as f:
    f.write(','.join(str(d) for d in durations))

print(f'Done! {img.n_frames} frames extracted to {out_dir}')
