using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GerenciadorSave : MonoBehaviour
{
    public static GerenciadorSave Instancia;

    [Header("Configuração")]
    [SerializeField] private string chave = "ChaveSave123";

    private TMP_Text textoSlot1;
    private TMP_Text textoSlot2;
    private TMP_Text textoSlot3;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        EncontrarTextosDosSlots();
        AtualizarStatusSlots();
    }


    // =========================================================
    // PROCURA OS TEXTOS DOS SLOTS AUTOMATICAMENTE
    // =========================================================

    private void EncontrarTextosDosSlots()
    {
        TMP_Text[] textos = FindObjectsByType<TMP_Text>(
            FindObjectsInactive.Include
        );

        foreach (TMP_Text texto in textos)
        {
            if (texto == null)
                continue;

            string textoAtual = texto.text.Trim();

            if (textoAtual == "Slot 1")
            {
                textoSlot1 = texto;
            }
            else if (textoAtual == "Slot 2")
            {
                textoSlot2 = texto;
            }
            else if (textoAtual == "Slot 3")
            {
                textoSlot3 = texto;
            }
        }

        Debug.Log(
            "Texto Slot 1 encontrado: " +
            (textoSlot1 != null)
        );

        Debug.Log(
            "Texto Slot 2 encontrado: " +
            (textoSlot2 != null)
        );

        Debug.Log(
            "Texto Slot 3 encontrado: " +
            (textoSlot3 != null)
        );
    }


    // =========================================================
    // UPDATE
    // Procura novamente caso a tela de slots ainda não existisse
    // =========================================================

    private void Update()
    {
        if (
            textoSlot1 == null ||
            textoSlot2 == null ||
            textoSlot3 == null
        )
        {
            EncontrarTextosDosSlots();

            if (
                textoSlot1 != null &&
                textoSlot2 != null &&
                textoSlot3 != null
            )
            {
                AtualizarStatusSlots();
            }
        }
    }


    // =========================================================
    // ATUALIZA STATUS DOS SLOTS
    // =========================================================

    public void AtualizarStatusSlots()
    {
        AtualizarSlot(textoSlot1, 1);
        AtualizarSlot(textoSlot2, 2);
        AtualizarSlot(textoSlot3, 3);
    }


    private void AtualizarSlot(TMP_Text texto, int slot)
    {
        if (texto == null)
            return;

        if (SlotExisteSemLog(slot))
        {
            texto.text = "Slot " + slot + " - Ocupado";
        }
        else
        {
            texto.text = "Slot " + slot + " - Vazio";
        }
    }


    private bool SlotExisteSemLog(int slot)
    {
        return File.Exists(ObterCaminho(slot));
    }


    // =========================================================
    // SALVAR
    // =========================================================

    public void Salvar(int slot, DadosSave dados)
    {
        if (dados == null)
        {
            Debug.LogError(
                "ERRO: Tentativa de salvar dados nulos."
            );

            return;
        }

        string json = JsonUtility.ToJson(dados, true);

        string textoCriptografado = Criptografar(json);

        string caminho = ObterCaminho(slot);

        File.WriteAllText(
            caminho,
            textoCriptografado
        );

        Debug.Log("=================================");
        Debug.Log("JOGO SALVO");
        Debug.Log("Slot: " + slot);
        Debug.Log("Fase: " + dados.nomeFase);
        Debug.Log(
            "Checkpoint: " +
            dados.checkpointAtivado
        );
        Debug.Log(
            "Moedas no checkpoint: " +
            dados.moedasNoCheckpoint
        );
        Debug.Log("Caminho: " + caminho);
        Debug.Log("=================================");

        AtualizarStatusSlots();
    }


    // =========================================================
    // CARREGAR
    // =========================================================

    public DadosSave Carregar(int slot)
    {
        string caminho = ObterCaminho(slot);

        if (!File.Exists(caminho))
        {
            Debug.LogWarning(
                "SLOT " +
                slot +
                " ESTÁ VAZIO."
            );

            return null;
        }

        try
        {
            string textoCriptografado =
                File.ReadAllText(caminho);

            string json =
                Descriptografar(textoCriptografado);

            DadosSave dados =
                JsonUtility.FromJson<DadosSave>(json);

            if (dados == null)
            {
                Debug.LogError(
                    "ERRO: Não foi possível converter " +
                    "o save do Slot " +
                    slot
                );

                return null;
            }

            Debug.Log("=================================");
            Debug.Log("DADOS DO SLOT CARREGADOS");
            Debug.Log("Slot: " + slot);
            Debug.Log("Fase: " + dados.nomeFase);
            Debug.Log(
                "Checkpoint: " +
                dados.checkpointAtivado
            );
            Debug.Log(
                "Moedas no checkpoint: " +
                dados.moedasNoCheckpoint
            );
            Debug.Log("=================================");

            return dados;
        }
        catch (System.Exception erro)
        {
            Debug.LogError(
                "ERRO AO CARREGAR SLOT " +
                slot +
                ": " +
                erro.Message
            );

            return null;
        }
    }


    // =========================================================
    // CARREGAR JOGO
    // =========================================================

    public void CarregarJogo(int slot)
    {
        Debug.Log("=================================");
        Debug.Log("INICIANDO CARREGAMENTO");
        Debug.Log("Slot escolhido: " + slot);
        Debug.Log("=================================");

        DadosSave dados = Carregar(slot);

        if (dados == null)
        {
            Debug.LogWarning(
                "Não foi possível carregar o Slot " +
                slot +
                " porque ele está vazio ou possui erro."
            );

            return;
        }


        // -----------------------------------------------------
        // COPIA SLOT MANUAL PARA O AUTOSAVE
        // -----------------------------------------------------

        if (slot != 0)
        {
            Debug.Log(
                "Copiando Slot " +
                slot +
                " para o Slot 0 (Autosave)."
            );

            Salvar(0, dados);
        }


        // -----------------------------------------------------
        // PREPARA DADOS PARA A CENA
        // -----------------------------------------------------

        PlayerPrefs.SetInt(
            "CarregandoSave",
            1
        );

        PlayerPrefs.SetInt(
            "CheckpointAtivado",
            dados.checkpointAtivado
                ? 1
                : 0
        );

        PlayerPrefs.SetInt(
            "MoedasCheckpoint",
            dados.moedasNoCheckpoint
        );


        // -----------------------------------------------------
        // MOEDAS COLETADAS NO CHECKPOINT
        // -----------------------------------------------------

        string moedas = "";

        if (
            dados.moedasColetadasCheckpoint != null
        )
        {
            moedas = string.Join(
                "|",
                dados.moedasColetadasCheckpoint
            );
        }

        PlayerPrefs.SetString(
            "MoedasColetadasCheckpoint",
            moedas
        );

        PlayerPrefs.Save();


        // -----------------------------------------------------
        // LOG
        // -----------------------------------------------------

        Debug.Log("=================================");
        Debug.Log("SAVE PRONTO PARA SER CARREGADO");
        Debug.Log("Slot: " + slot);
        Debug.Log("Fase: " + dados.nomeFase);
        Debug.Log(
            "Checkpoint ativado: " +
            dados.checkpointAtivado
        );
        Debug.Log(
            "Moedas do checkpoint: " +
            dados.moedasNoCheckpoint
        );
        Debug.Log("=================================");


        // -----------------------------------------------------
        // CARREGA A FASE
        // -----------------------------------------------------

        SceneManager.LoadScene(
            dados.nomeFase
        );
    }


    // =========================================================
    // VERIFICAR SLOT
    // =========================================================

    public bool SlotExiste(int slot)
    {
        bool existe =
            File.Exists(
                ObterCaminho(slot)
            );

        Debug.Log(
            "Verificando Slot " +
            slot +
            " → " +
            (
                existe
                    ? "OCUPADO"
                    : "VAZIO"
            )
        );

        return existe;
    }


    // =========================================================
    // APAGAR SLOT
    // =========================================================

    public void ApagarSlot(int slot)
    {
        string caminho =
            ObterCaminho(slot);

        if (File.Exists(caminho))
        {
            File.Delete(caminho);

            Debug.Log(
                "Slot " +
                slot +
                " apagado com sucesso."
            );
        }
        else
        {
            Debug.Log(
                "Slot " +
                slot +
                " já estava vazio."
            );
        }

        AtualizarStatusSlots();
    }


    // =========================================================
    // CAMINHO DO SAVE
    // =========================================================

    private string ObterCaminho(int slot)
    {
        return Path.Combine(
            Application.persistentDataPath,
            "save_slot_" +
            slot +
            ".dat"
        );
    }


    // =========================================================
    // CRIPTOGRAFAR
    // =========================================================

    private string Criptografar(string texto)
    {
        byte[] dados =
            Encoding.UTF8.GetBytes(texto);

        byte[] chaveBytes =
            Encoding.UTF8.GetBytes(chave);

        for (
            int i = 0;
            i < dados.Length;
            i++
        )
        {
            dados[i] ^=
                chaveBytes[
                    i % chaveBytes.Length
                ];
        }

        return System.Convert
            .ToBase64String(dados);
    }


    // =========================================================
    // DESCRIPTOGRAFAR
    // =========================================================

    private string Descriptografar(string texto)
    {
        byte[] dados =
            System.Convert.FromBase64String(
                texto
            );

        byte[] chaveBytes =
            Encoding.UTF8.GetBytes(chave);

        for (
            int i = 0;
            i < dados.Length;
            i++
        )
        {
            dados[i] ^=
                chaveBytes[
                    i % chaveBytes.Length
                ];
        }

        return Encoding.UTF8.GetString(
            dados
        );
    }
}