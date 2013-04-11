using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonTypes
{
    public interface IPuppetToClient
    {
        //puppet envia informações ao cliente
        //void guardaMS(Hashtable metadataservers);

        //puppet manda o Cliente executar accoes
        void runScript(List<string> operations);
        void dump();
        
        //puppet manda o cliente enviar pedidos ao MS
        void open(string fileName);     
        void close(string fileName);    
        void create(string fileName, int numDS, int rQuorum, int wQuorum);  
        void delete(string fileName);
        void copy(int fileRegister1, string semantics, int fileRegister2, string salt);
        

        //puppet mando o cliente enviar pedidos ao DS
        void read(int fileRegister, string semantics, int stringRegister); 
        void writeR(int fileRegister, int ByteArrayRegister);
        void writeS(int fileRegister, string conteudo);
    }

    public interface IPuppetToDS
    {
        void freeze();   //starts buffering read and write requests, without answering
        void unfreeze(); //responds to all buffered requests from clients and restarts replying new requests
        void fail();     //DS ignores requests from Clients or messages from MS
        void recover();  //DS starts receiving requests from Clients and MS
        void dump();
    }

    public interface IPuppetToMS
    {
        void fail();    //the MS stops processing requests from clients or others MS
        void recover(); //MS starts receiving requests from clients and others MS
        void dump();

    }

    public interface IClientToPuppet
    {
        void respostaClient (string resposta);
    }

    public interface IClientToMS
    {
        DadosFicheiro open(string fileName);     //returns to client the contents of the metadata stored for that file
        void close(string fileName);    //informs MS that client is no longer using that file - client must discard all metadata for that file
        DadosFicheiro create(string fileName, int numDS, int rQuorum, int wQuorum);  //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        DadosFicheiro delete(string fileName);   //deletes the file
        void confirmarDelete(string fileName, bool confirmacao);
    }

    public interface IClientToDS
    {
        DadosFicheiroDS read(string fileName, string semantics); //returns the version and content os local file
        void write(string fileName, byte[] array); //overwrites the content of file, creates new version
        bool delete(string fileName);
        void confirmarDelete(string fileName, bool confirmacao);
    }

    public interface IDSToPuppet
    {
        void respostaDS(string resposta);
    }

    public interface IDSToMS
    {
        void respostaDS(string resposta);
        void registarDS(string nome, string id);
    }

    public interface IDSToClient
    {
        void respostaDS(string resposta);
    }

    public interface IMSToPuppet
    {
        void respostaMS(string resposta);
    }

    public interface IMSToClient
    {
        void respostaMS(string resposta);
    }

    public interface IMSToDS
    {
        void areYouAlive();
        void respostaMS(string resp);
    }

    public interface IMSToMS
    {
        //MS
        bool areYouAlive();
        Hashtable get_dataServers();
        SortedDictionary<string, int> get_DSnum();
        Hashtable get_files();
        Hashtable get_nBDataS();
       
        //DS
        void registarDS_replica(string nome, string id);

        //client
        void create_replica(DadosFicheiro file, int numDS);  //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        void confirmarDelete_replica(string fileName, bool confirmacao);
    }

    [Serializable]
    public class DadosFicheiro{

        int rQ;
        int wQ;
        Hashtable ports;

        public DadosFicheiro(int rQ, int wQ, Hashtable ports){

            this.rQ = rQ;
            this.wQ = wQ;
            this.ports = ports;
        }

        public int getRQ()
        {
            return rQ;
        }

        public int getWQ()
        {
            return wQ;
        }

        public Hashtable getPorts()
        {
            return ports;
        }    
    }

    [Serializable]
    public class DadosFicheiroDS
    {
        int version;
        byte[] ficheiro;

        public DadosFicheiroDS() { }

        public DadosFicheiroDS(int version, byte[] ficheiro)
        {
            this.version = version;
            this.ficheiro = ficheiro;
        }

        public int getVersion()
        {
            return version;
        }

        public byte[] getFile()
        {
            return ficheiro;
        }
    }
}
