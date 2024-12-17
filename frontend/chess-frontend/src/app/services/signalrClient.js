import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

export const connectToHub = async (url, onHandlers) => {
  const connection = new HubConnectionBuilder()
    .withUrl(url)
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();

  Object.keys(onHandlers).forEach((event) => {
    connection.on(event, onHandlers[event]);
  });

  await connection.start();
  return connection;
};
