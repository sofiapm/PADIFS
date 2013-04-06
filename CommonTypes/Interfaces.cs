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
        void guardaMS(Hashtable metadataservers);

        //puppet manda o Cliente executar accoes
        void runScript(List<string> operations);
        void freeze();   //starts buffering read and write requests, without answering
        void unfreeze(); //responds to all buffered requests from clients and restarts replying new requests
        void fail();     
        void recover();  
        
        //puppet manda o cliente enviar pedidos ao MS
        void open(string fileName);     
        void close(string fileName);    
        void create(string fileName, int numDS, int rQuorum, int wQuorum);  
        void delete(string fileName);
        

        //puppet mando o cliente enviar pedidos ao DS
        void read(string fileName, string semantics); 
        void write(string fileName, byte[] array);
    }

    public interface IPuppetToDS
    {
        void freeze();   //starts buffering read and write requests, without answering
        void unfreeze(); //responds to all buffered requests from clients and restarts replying new requests
        void fail();     //DS ignores requests from Clients or messages from MS
        void recover();  //DS starts receiving requests from Clients and MS
    }

    public interface IPuppetToMS
    {
        void fail();    //the MS stops processing requests from clients or others MS
        void recover(); //MS starts receiving requests from clients and others MS
        void freeze();   //starts buffering read and write requests, without answering
        void unfreeze(); //responds to all buffered requests from clients and restarts replying new requests

    }

    public interface IClientToPuppet
    {
        void respostaClient (string resposta);
    }

    public interface IClientToMS
    {
        void open(string fileName);     //returns to client the contents of the metadata stored for that file
        void close(string fileName);    //informs MS that client is no longer using that file - client must discard all metadata for that file
        void create(string fileName, int numDS, int rQuorum, int wQuorum);  //creates a new file (if it doesn t exist) - in case of sucesses, returns the same that open
        void delete(string fileName);   //deletes the file
    }

    public interface IClientToDS
    {
        void read (string fileName, string semantics); //returns the version and content os local file
        void write(string fileName, byte[] array); //overwrites the content of file, creates new version
    }

    public interface IDSToPuppet
    {
        void respostaDS(string resposta);
    }

    public interface IDSToMS
    {
        void respostaDS(string resposta);
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
        void guardaDS(Hashtable dataservers);
    }

    public interface IMSToDS
    {
        void areYouAlive();
        void respostaMS(string resp);
    }

    
}
