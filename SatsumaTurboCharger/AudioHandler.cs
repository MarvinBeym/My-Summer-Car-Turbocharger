using System;
using System.Collections.Generic;
using System.IO;
using MSCLoader;
using MscModApi.Parts;
using MscModApi.Parts.EventSystem;
using MscModApi.Tools;
using UnityEngine;

namespace SatsumaTurboCharger.turbo
{
	public class AudioHandler
	{

		protected Dictionary<string, AudioSource> noiseStorage = new Dictionary<string, AudioSource>();

		protected readonly Mod mod;

		public AudioHandler(Mod mod)
		{
			this.mod = mod;
		}

		protected void Add(string id, AudioSource audioSource)
		{
			audioSource.playOnAwake = false;
			audioSource.Stop();
			noiseStorage.Add(id, audioSource);

		}

		public void SetVolume(AudioSource audioSource, float volume)
		{
			if (audioSource == null)
			{
				return;
			}

			audioSource.volume = volume;
		}

		public void SetVolume(string id, float volume)
		{
			SetVolume(Get(id), volume);
		}

		public void SetPitch(AudioSource audioSource, float pitch)
		{
			if (audioSource == null)
			{
				return;
			}

			audioSource.pitch = pitch;
		}

		public void SetPitch(string id, float pitch)
		{
			SetPitch(Get(id), pitch);
		}

		public void Add(string id, Part part, string fileName, PartEvent.Type eventTypeWhenActive, bool loop = false)
		{
			if (fileName == "")
			{
				Logger.New("No fileName given for loading audio");
				return;
			}

			AudioSource audioSource = part.AddEventBehaviour<AudioSource>(eventTypeWhenActive);
			ModAudio modAudio = new ModAudio
			{
				audioSource = audioSource
			};
			modAudio.LoadAudioFromFile(Path.Combine(ModLoader.GetModAssetsFolder(mod), fileName), true, false);
			audioSource.minDistance = 1;
			audioSource.maxDistance = 10;
			audioSource.spatialBlend = 1;
			audioSource.loop = loop;
			Add(id, audioSource);
		}

		public AudioSource Get(string id)
		{
			return noiseStorage.TryGetValue(id, out AudioSource audio) ? audio : null;
		}

		public void Play(AudioSource audioSource)
		{
			if (audioSource == null)
			{
				return;
			}

			if (audioSource.isPlaying)
			{
				return;
			}

			audioSource.Play();
		}

		public void Play(string id)
		{
			Play(Get(id));
		}

		public void Stop(string id)
		{
			Stop(Get(id));
		}

		protected void Stop(AudioSource audioSource)
		{
			if (audioSource == null)
			{
				return;
			}

			if (!audioSource.isPlaying)
			{
				return;
			}

			audioSource.Stop();
		}

		public void StopAll()
		{
			foreach (var keyValue in noiseStorage)
			{
				Stop(keyValue.Value);
			}
		}
	}
}