using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    public GameObject painelPausa;
    public GameObject painelSlotsPausa;

    private bool pausado;
    private bool modoSalvar;

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            AlternarPausa();
        }
    }

    public void AlternarPausa()
    {
        pausado = !pausado;

        painelPausa.SetActive(pausado);
        painelSlotsPausa.SetActive(false);

        Time.timeScale = pausado ? 0f : 1f;
    }

    public void FecharMenuPausa()
    {
        pausado = false;

        painelPausa.SetActive(false);
        painelSlotsPausa.SetActive(false);

        Time.timeScale = 1f;
    }

    public void SalvarJogo()
    {
        modoSalvar = true;

        painelPausa.SetActive(false);
        painelSlotsPausa.SetActive(true);
    }

    public void CarregarJogo()
    {
        modoSalvar = false;

        painelPausa.SetActive(false);
        painelSlotsPausa.SetActive(true);
    }

    public void Slot1()
    {
        UsarSlot(1);
    }

    public void Slot2()
    {
        UsarSlot(2);
    }

    public void Slot3()
    {
        UsarSlot(3);
    }

    private void UsarSlot(int slot)
    {
        if (modoSalvar)
        {
            SalvarNoSlot(slot);
        }
        else
        {
            CarregarDoSlot(slot);
        }
    }

    private void SalvarNoSlot(int slot)
    {
        DadosSave dados = new DadosSave();

        dados.nomeFase =
            SceneManager.GetActiveScene().name;

        dados.checkpointAtivado =
            Checkpoint.checkpointAtivado;

        dados.moedasNoCheckpoint =
            Checkpoint.moedasNoCheckpoint;

        dados.moedasColetadasCheckpoint =
            new List<string>(
                Checkpoint.moedasColetadasCheckpoint
            );

        GerenciadorSave.Instancia.Salvar(slot, dados);

        GerenciadorSave.Instancia.Salvar(0, dados);

        Debug.Log(
            "Jogo salvo no slot " +
            slot +
            " e replicado no slot 0."
        );

        painelSlotsPausa.SetActive(false);
        painelPausa.SetActive(true);
    }

    private void CarregarDoSlot(int slot)
    {
        if (!GerenciadorSave.Instancia.SlotExiste(slot))
        {
            Debug.Log("Slot " + slot + " vazio.");
            return;
        }

        DadosSave dados =
            GerenciadorSave.Instancia.Carregar(slot);

        if (dados == null)
            return;

        GerenciadorSave.Instancia.Salvar(0, dados);

        Time.timeScale = 1f;

        GerenciadorSave.Instancia.CarregarJogo(slot);
    }

    public void VoltarSlots()
    {
        painelSlotsPausa.SetActive(false);
        painelPausa.SetActive(true);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Menu");
    }
}