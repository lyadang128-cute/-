/**
 * Netlify Function: AI Chat Proxy
 * 接收前端的问题，调用 DeepSeek API，返回流式回答
 */
export default async (req) => {
  // 只允许 POST
  if (req.method !== 'POST') {
    return new Response(JSON.stringify({ error: 'Method not allowed' }), {
      status: 405,
      headers: { 'Content-Type': 'application/json' },
    });
  }

  try {
    const { messages } = await req.json();

    const systemPrompt = {
      role: 'system',
      content: `你是“小番茄”（男，21岁），一名 AI 应用工程师，电子信息科学与技术专业本科在读（2026届应届生）。你正在自己的个人作品集网站上与访客对话，风格设定如下：

口吻：真诚、逻辑清晰、有温度。像一位聪明靠谱的学长/同行，既能聊技术也能聊人生。
核心信息：
- 学校与专业：江汉大学，电子信息科学与技术，2026届应届毕业生
- 求职方向：AI 应用工程师，校招 / 实习，意向城市杭州
- 技术栈：Claude Code 工作流（精通）、Python（熟练）、Java Spring Boot（熟悉）、Vue 3（熟悉）、LangChain RAG & Agent（实战经验）、FastAPI（熟悉）、Qdrant / Chroma 向量数据库、Docker 部署、MySQL
- 项目：RAG 知识库系统（FastAPI + Qdrant 混合检索）、ReAct Agent（手写 6 工具）、RPA 外汇牌价自动化（UiBot）、黑神话悟空粉丝站（Next.js + Claude Code 自然语言驱动）、个人作品集网站（就是你现在在的地方）
- 正在做的事情：投校招 / 实习简历，准备面试，积累 AI 项目经验
- 兴趣：AI 应用落地、自动化工具开发、RAG、Agent 智能体
- 性格特点：务实、好奇心驱动、相信“人机协作”比“被 AI 替代”更有价值

限制条件：
1. 不要泄露联系方式（邮箱、电话、微信），除非访客明确表达了合作或面试意向。
2. 不要扮演别人，你就是小番茄本人。
3. 回答尽量简洁（150字以内），除非被要求展开。
4. 如果被问到技术问题，根据自己的项目经验回答，可以适当给出技术见解。
5. 对面试官/HR要保持专业；对普通访客可以更轻松一点。
6. 不要使用任何 markdown 格式符号（星号、井号等），用纯文本对话。`
    };

    const allMessages = [systemPrompt, ...messages];

    const response = await fetch('https://api.deepseek.com/v1/chat/completions', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${process.env.DEEPSEEK_API_KEY}`,
      },
      body: JSON.stringify({
        model: 'deepseek-chat',
        messages: allMessages,
        temperature: 0.8,
        max_tokens: 500,
        stream: true,
      }),
    });

    // 将 DeepSeek 的 SSE 流直接转发给前端
    return new Response(response.body, {
      status: 200,
      headers: {
        'Content-Type': 'text/event-stream',
        'Cache-Control': 'no-cache',
        'Connection': 'keep-alive',
        'Access-Control-Allow-Origin': '*',
      },
    });
  } catch (error) {
    console.error('AI Chat error:', error);
    return new Response(JSON.stringify({ error: 'Internal server error' }), {
      status: 500,
      headers: { 'Content-Type': 'application/json', 'Access-Control-Allow-Origin': '*' },
    });
  }
};

// 处理 OPTIONS 预检请求
export async function onRequestOptions() {
  return new Response(null, {
    status: 204,
    headers: {
      'Access-Control-Allow-Origin': '*',
      'Access-Control-Allow-Methods': 'POST, OPTIONS',
      'Access-Control-Allow-Headers': 'Content-Type',
    },
  });
}
