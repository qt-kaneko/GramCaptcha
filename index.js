function callback(token)
{
  let url = new URL(location.href);
  let searchParams = url.searchParams;

  let chatId = Number(searchParams.get(`start_param`));

  let data = {
    chat_id: chatId,
    token: token
  };
  let dataJson = JSON.stringify(data);

  console.clear();
  console.log("BEFORE SEND_DATA", dataJson);
  window.Telegram.WebApp.sendData(dataJson);
  console.log("AFTER SEND_DATA", dataJson);
}