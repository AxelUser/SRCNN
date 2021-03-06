﻿using System;
using System.Threading.Tasks;
using ImageSuperResolution.Common.Messages.QueueEvents;
using EasyNetQ;
using ImageSuperResolution.Common.Messages.QueueCommands;
using ImageSuperResolution.Common;
using System.IO;
using LiteDB;
using System.Collections.Generic;
using System.Linq;
using ImageSuperResolution.Common.Messages.ViewModels;
using System.Drawing;

namespace ImageSuperResolution.Web.Servicies
{
    public class UpscallingService : IUpscallingService, IDisposable
    {
        private IBus _mqBus;
        private IDisposable progressReceiver;
        private IDisposable resultReceiver;
        private readonly string _dataFolder = "DataStorage";
        private readonly string _dbName = "upscalling.db";
        private readonly string _dbFilePath;
        private readonly BsonMapper _dbMapper;
        private readonly LiteDatabase _db;


        public UpscallingService(bool shallClearData = false)
        {
            _mqBus = RabbitHutch.CreateBus("host=localhost");

            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), _dataFolder));
            _dbFilePath = Path.Combine(Directory.GetCurrentDirectory(), _dataFolder, _dbName);

            if (shallClearData)
            {
                ClearData();
            }

            _dbMapper = new BsonMapper();
            _dbMapper.Entity<TaskProgress>().Id(tp => tp.MessageId);
            _dbMapper.Entity<TaskFinished>().Id(tp => tp.TaskId).Ignore(tp => tp.Image);

            progressReceiver = _mqBus.Receive<TaskProgress>(MqUtils.UpscallingProgressQueue, m => OnProgress(m));
            resultReceiver = _mqBus.Receive<TaskFinished>(MqUtils.UpscallingResultQueue, m => OnResult(m));

            _db = new LiteDatabase(new MemoryStream(), _dbMapper);
        }

        private void OnProgress(TaskProgress progressMessage)
        {
            Task.Run(() =>
            {
                lock (_db)
                {
                    var progressEvents = _db.GetCollection<TaskProgress>();
                    progressEvents.Insert(progressMessage);
                }
            });
        }

        private void OnResult(TaskFinished resultMessage)
        {
            Task.Run(() =>
            {
                lock (_db)
                {
                    var progressEvents = _db.GetCollection<TaskFinished>();
                    progressEvents.Insert(resultMessage);

                    using (var ms = new MemoryStream(resultMessage.Image))
                    {
                        var image = new Bitmap(ms);
                        image.Save(Path.Combine(_dataFolder, $"{resultMessage.TaskId}.png"), System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            });
        }

        public IEnumerable<TaskProgress> GetProgress(Guid ticket)
        {
            var data = _db.GetCollection<TaskProgress>().FindAll();
            var cols = _db.GetCollectionNames();
            var progressEvents = _db.GetCollection<TaskProgress>().Find(p => p.TaskId == ticket).ToList();
            return progressEvents;
        }

        public ResultInfo GetResultInfo(Guid ticket)
        {
            var resultEvent = _db.GetCollection<TaskFinished>().FindById(ticket);
            if (resultEvent != null)
            {
                var pathToFile = Path.Combine(_dataFolder, $"{ticket}.png");
                if (File.Exists(pathToFile))
                {
                    string fileUrl = $"/image/{ticket}.png";
                    return new ResultInfo()
                    {
                        Height = resultEvent.Height,
                        Width = resultEvent.Width,
                        ElapsedTime = resultEvent.ElapsedTime,
                        FilePath = fileUrl
                    };
                }
            }
            return null;
        }

        public async Task<Guid> SendFile(byte[] image)
        {
            var taskId = Guid.NewGuid();
            await _mqBus.SendAsync(MqUtils.ImageForUpscallingQueue, new SendImage()
            {
                TaskId = taskId,
                Image = image
            });
            return taskId;
        }

        public void Dispose()
        {
            progressReceiver?.Dispose();
            resultReceiver?.Dispose();
            _db.Dispose();
        }

        public bool ClearEvents(Guid ticket)
        {
            try
            {
                _db.GetCollection<TaskProgress>().Delete(tp => tp.TaskId == ticket);
                _db.GetCollection<TaskFinished>().Delete(tf => tf.TaskId == ticket);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void ClearData()
        {
            var filePaths = Directory.GetFiles(_dataFolder);
            foreach (var filePath in filePaths)
            {
                File.Delete(filePath);
            }
        }
    }
}
