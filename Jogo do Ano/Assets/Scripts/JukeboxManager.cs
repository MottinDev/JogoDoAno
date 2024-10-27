using UnityEngine;
using UnityEngine.UI;  
using TMPro;          

public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource;         
    public AudioClip[] musicClips;          
    public Slider volumeSlider;            
    public Button playPauseButton;         
    public TMP_Text trackNameText;         

    private int currentTrackIndex = 0;      
    private bool isPaused = false;          

    void Start()
    {
        if (musicClips.Length > 0)
        {
            PlayMusic(currentTrackIndex);
        }

        // Configura o volume inicial com base no slider
        audioSource.volume = volumeSlider.value;

        // Detecta mudan�as no slider de volume
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Adiciona listener ao bot�o Play/Pause
        playPauseButton.onClick.AddListener(TogglePlayPause);

        // Desativa o loop de cada m�sica para poder mudar automaticamente
        audioSource.loop = false;
    }

    void Update()
    {
        // Verifica se a m�sica terminou
        if (!audioSource.isPlaying && !isPaused)
        {
            NextTrack();
        }
    }

    // Fun��o para tocar a m�sica
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
        isPaused = false;

        // Atualiza o nome da m�sica no UI (TextMeshPro)
        trackNameText.text = musicClips[index].name;
    }

    // Fun��o para alternar entre Play e Pause
    public void TogglePlayPause()
    {
        // Verifica se o �udio est� pausado ou tocando
        if (isPaused)
        {
            // Retoma a reprodu��o
            audioSource.Play();
            isPaused = false;
        }
        else if (audioSource.isPlaying)
        {
            // Pausa a reprodu��o
            audioSource.Pause();
            isPaused = true;
        }
        else
        {
            // Caso o �udio tenha parado, toca novamente
            audioSource.Play();
            isPaused = false;
        }
    }

    // Trocar para a pr�xima m�sica
    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length; // Avan�a para o pr�ximo, com loop
        PlayMusic(currentTrackIndex);
    }

    // Trocar para a m�sica anterior
    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicClips.Length - 1; // Volta para a �ltima m�sica se for menor que 0
        }
        PlayMusic(currentTrackIndex);
    }

    // Regular o volume
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}
