using Domino;
using NLog;
using System;

namespace NotesReplicatorX
{
    class Replicator
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public string SourceServer { get; set; }
        public string TargetServer { get; set; }

        public string SourceFolder { get; set; }

        public Replicator(string sourceServer, string targetServer, string sourceFolder)
        {
            SourceServer = sourceServer ?? throw new ArgumentNullException(nameof(sourceServer));
            TargetServer = targetServer ?? throw new ArgumentNullException(nameof(targetServer));
            SourceFolder = sourceFolder ?? throw new ArgumentNullException(nameof(sourceFolder));
        }

        public void Replicate()
        {
            NotesSession session = new NotesSession();
            session.Initialize();
            NotesDbDirectory dbDir = session.GetDbDirectory(SourceServer);
            NotesDatabase db = dbDir.GetFirstDatabase(DB_TYPES.NOTES_DATABASE);
            while (db != null)
            {
                if (db.FilePath.ToLower().Contains(SourceFolder.ToLower()))
                    try
                    {
                        if (db.OpenByReplicaID(TargetServer, db.ReplicaID))
                        {
                            logger.Info($"Replicating db: {db.FilePath}");

                            if (db.Replicate(TargetServer))
                            {
                                logger.Info("Ok, database replicated");
                            }
                            else
                            {
                                logger.Warn("Error replicating database");
                            }

                        }
                        else
                        {
                            logger.Info($"Creating new replica: {db.FilePath}");
                            db.CreateReplica(TargetServer, db.FilePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, ex.Message);
                    }
            }

            db = dbDir.GetNextDatabase();
        }
    }
}
