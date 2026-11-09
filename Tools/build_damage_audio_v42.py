"""Deterministic, original player damage cues; no downloaded samples.

Shield: dry high glass tick. Health: low body/cloth thud.
Break: staggered bright cracks with descending crystal debris.
Writes higher-priority V25 overrides without replacing the legacy sound bank.
"""
from pathlib import Path
import hashlib
import json
import uuid
import wave
import numpy as np
from scipy.signal import butter, sosfilt

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Assets/NakiPrototype/Resources/NakiSoundV25'
QA = ROOT.parent / 'Validation/FeedbackV42'
RATE = 44100


def band(noise, low, high):
    return sosfilt(butter(3, [low, high], fs=RATE, btype='bandpass', output='sos'), noise)


def build(kind, variant):
    duration = {'shield': .17, 'hurt': .29, 'break': .66}[kind]
    t = np.arange(round(duration * RATE)) / RATE
    rng = np.random.default_rng(4200 + variant * 13 + {'shield': 1, 'hurt': 2, 'break': 3}[kind])
    noise = rng.normal(0, 1, len(t))
    pitch = 1 + (variant - 1) * .025
    if kind == 'shield':
        x = (.5 * np.sin(2 * np.pi * 1550 * pitch * t) * np.exp(-t * 42)
             + .22 * np.sin(2 * np.pi * 2370 * pitch * t) * np.exp(-t * 55)
             + .1 * band(noise, 1400, 6500) * np.exp(-t * 85))
    elif kind == 'hurt':
        phase = 2 * np.pi * pitch * (80 * t + 100 * (1 - np.exp(-t * 26)) / 26)
        x = (.65 * np.sin(phase) * np.exp(-t * 21)
             + .45 * band(noise, 70, 650) * np.exp(-t * 28)
             + .16 * band(noise, 180, 1600) * np.exp(-t * 17))
    else:
        x = np.zeros_like(t)
        for at, level in [(0, 1), (.055, .7), (.12, .45)]:
            u = np.maximum(0, t - at)
            x += (t >= at) * level * band(noise, 1900, 9000) * np.exp(-u * 60)
        for freq, decay in [(2800, 9), (1910, 12), (970, 16)]:
            x += .16 * np.sin(2 * np.pi * pitch * freq * (1 - np.exp(-t * 2.2)) / 2.2) * np.exp(-t * decay)
        x += .27 * np.sin(2 * np.pi * 95 * t) * np.exp(-t * 35)
    # Smooth attack/end, remove DC, preserve ample mix headroom.
    x *= np.minimum(t / .0015, 1) * np.minimum((duration - t) / .025, 1)
    x -= x.mean()
    target = {'shield': .115, 'hurt': .15, 'break': .145}[kind]
    x *= min(target / max(1e-8, np.sqrt(np.mean(x*x))), .8 / max(1e-8, np.max(np.abs(x))))
    return x


def write(path, x):
    with wave.open(str(path), 'wb') as f:
        f.setnchannels(1)
        f.setsampwidth(2)
        f.setframerate(RATE)
        f.writeframes((x * 32767).astype('<i2').tobytes())


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    QA.mkdir(parents=True, exist_ok=True)
    template = (ROOT / 'Assets/NakiPrototype/Resources/NakiAudio/shield_0.wav.meta').read_text(encoding='utf-8-sig')
    report = []
    for kind in ['shield', 'hurt', 'break']:
        for variant in range(3):
            x = build(kind, variant)
            path = OUT / f'{kind}_{variant}.wav'
            write(path, x)
            meta = Path(str(path) + '.meta')
            if not meta.exists():
                import re
                meta.write_text(re.sub(r'guid: [a-f0-9]+', 'guid: ' + uuid.uuid4().hex, template, count=1), encoding='utf-8')
            magnitude = np.abs(np.fft.rfft(x))
            frequencies = np.fft.rfftfreq(len(x), 1/RATE)
            report.append({'key': path.stem, 'seconds': len(x)/RATE,
                           'peak': float(np.max(np.abs(x))), 'rms': float(np.sqrt(np.mean(x*x))),
                           'centroid_hz': float(np.sum(magnitude*frequencies)/np.sum(magnitude)),
                           'sha256': hashlib.sha256(path.read_bytes()).hexdigest()})
    preview = np.concatenate([np.zeros(round(RATE*.25))] +
                             [part for kind in ['shield', 'hurt', 'break']
                              for part in [build(kind, 1), np.zeros(round(RATE*.55)),
                                           build(kind, 0), np.zeros(round(RATE*.8))]])
    write(QA/'damage-cues-preview.wav', preview)
    (QA/'damage-audio-metrics.json').write_text(json.dumps({'sample_rate': RATE, 'source': 'Original deterministic synthesis', 'preview_order': ['shield twice', 'health twice', 'break twice'], 'cues': report}, indent=2), encoding='utf-8')
    print('Generated nine distinct damage cues and ordered preview.')


if __name__ == '__main__':
    main()
