using System;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Exceptions;
using Headstart.Common.Models;
using OrderCloud.Catalyst;

namespace Headstart.Common.Utils
{
    public static class ProcessAction
    {
        public static async Task<Tuple<ProcessResultAction, T>> Execute<T>(ProcessType type, string description, Task<T> func) where T : class, new()
        {
            // T must be a class and be newable so the error response can be handled.
            try
            {
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
                    {
                        ProcessType = type,
                        Description = description,
                        Success = true,
                    },
                    await func);
            }
            catch (CatalystBaseException integrationEx)
            {
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
                    {
                        Description = description,
                        ProcessType = type,
                        Success = false,
                        Exception = new ProcessResultException(integrationEx),
                    }, new T());
            }
            catch (FlurlHttpException flurlEx)
            {
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
                    {
                        Description = description,
                        ProcessType = type,
                        Success = false,
                        Exception = new ProcessResultException(flurlEx),
                    }, new T());
            }
            catch (Exception ex)
            {
                return new Tuple<ProcessResultAction, T>(
                    new ProcessResultAction()
                    {
                        Description = description,
                        ProcessType = type,
                        Success = false,
                        Exception = new ProcessResultException(ex),
                    }, new T());
            }
        }

        public static async Task<ProcessResultAction> Execute(ProcessType type, string description, Task func)
        {
            try
            {
                await func;
                return new ProcessResultAction()
                {
                    ProcessType = type,
                    Description = description,
                    Success = true,
                };
            }
            catch (CatalystBaseException integrationEx)
            {
                return new ProcessResultAction()
                {
                    Description = description,
                    ProcessType = type,
                    Success = false,
                    Exception = new ProcessResultException(integrationEx),
                };
            }
            catch (FlurlHttpException flurlEx)
            {
                return new ProcessResultAction()
                {
                    Description = description,
                    ProcessType = type,
                    Success = false,
                    Exception = new ProcessResultException(flurlEx),
                };
            }
            catch (Exception ex)
            {
                return new ProcessResultAction()
                {
                    Description = description,
                    ProcessType = type,
                    Success = false,
                    Exception = new ProcessResultException(ex),
                };
            }
        }
    }
}
