using UnityEngine;
using UnityEngine.UI;  // Adicione esta linha para corrigir o erro de Button e Slider
using TMPro;          // Para o TextMeshPro


public class JukeboxManager : MonoBehaviour
{
    public AudioSource audioSource;         // Controla a música que será tocada
    public AudioClip[] musicClips;          // Array de músicas disponíveis
    public Slider volumeSlider;             // Slider para controlar o volume
    public Button playPauseButton;          // Botão Play/Pause
    public TMP_Text trackNameText;          // Texto onde o nome da música será exibido (TMP)
    public float scrollSpeed = 50f;         // Velocidade do texto de deslizar

    private int currentTrackIndex = 0;      // Índice da música atual
    private bool isPaused = false;          // Indica se a música está pausada
    private RectTransform trackTextRect;    // Referência ao RectTransform do texto

    void Start()
    {
        if (musicClips.Length > 0)
        {
            PlayMusic(currentTrackIndex);
        }

        // Configura o volume inicial com base no slider
        audioSource.volume = volumeSlider.value;

        // Detecta mudanças no slider de volume
        volumeSlider.onValueChanged.AddListener(SetVolume);

        // Adiciona listener ao botão Play/Pause
        playPauseButton.onClick.AddListener(TogglePlayPause);

        // Referência ao RectTransform do texto (TextMeshPro)
        trackTextRect = trackNameText.GetComponent<RectTransform>();

        // Desativa o loop de cada música para poder mudar automaticamente
        audioSource.loop = false;
    }

    void Update()
    {
        // Verifica se a música terminou
        if (!audioSource.isPlaying && !isPaused)
        {
            NextTrack();
        }

        // Aplicar o efeito de deslizamento no nome da música
        ScrollTrackName();
    }

    // Função para tocar a música
    public void PlayMusic(int index)
    {
        audioSource.clip = musicClips[index];
        audioSource.Play();
        isPaused = false;

        // Atualiza o nome da música no UI (TextMeshPro)
        trackNameText.text = musicClips[index].name;

        // Resetar a posição do texto para o início
        trackTextRect.anchoredPosition = new Vector2(0, trackTextRect.anchoredPosition.y);
    }

    // Função para alternar entre Play e Pause
    public void TogglePlayPause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            isPaused = true;
        }
        else
        {
            audioSource.Play();
            isPaused = false;
        }
    }

    // Trocar para a próxima música
    public void NextTrack()
    {
        currentTrackIndex = (currentTrackIndex + 1) % musicClips.Length; // Avança para o próximo, com loop
        PlayMusic(currentTrackIndex);
    }

    // Trocar para a música anterior
    public void PreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = musicClips.Length - 1; // Volta para a última música se for menor que 0
        }
        PlayMusic(currentTrackIndex);
    }

    // Regular o volume
    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    // Função para deslizar o texto do nome da música
    private void ScrollTrackName()
    {
        if (trackNameText.preferredWidth > trackTextRect.rect.width)
        {
            // Desliza o texto para a esquerda
            trackTextRect.anchoredPosition += Vector2.left * scrollSpeed * Time.deltaTime;

            // Se o texto sair completamente da tela, volta ao início
            if (trackTextRect.anchoredPosition.x + trackNameText.preferredWidth < 0)
            {
                trackTextRect.anchoredPosition = new Vector2(trackTextRect.rect.width, trackTextRect.anchoredPosition.y);
            }
        }
    }
}
